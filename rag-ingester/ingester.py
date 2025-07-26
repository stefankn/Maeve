from chromadb import HttpClient, Settings
from chromadbx import UUIDGenerator
from langchain_community.document_loaders import PyMuPDFLoader, TextLoader
from langchain_chroma import Chroma
from langchain_ollama import OllamaEmbeddings
import os
import logging
import requests
import json

from langchain_text_splitters import RecursiveCharacterTextSplitter
from redis import Redis

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class Ingester:

    def __init__(self):
        embedding_model = os.getenv("OLLAMA_EMBEDDING_MODEL", "mxbai-embed-large")
        self.embeddings = OllamaEmbeddings(
            model=embedding_model,
            base_url=os.getenv("OLLAMA_URL", "http://host.docker.internal:11434")
        )

    def ingest(self, document: str, file_hash: str):
        logger.info(f"Starting ingestion file: {document}, hash: {file_hash}")

        redis = Redis(
            host=os.getenv("REDIS_HOST", "maeve-redis"),
            port=os.getenv("REDIS_PORT", 6379),
            decode_responses=True
        )

        redis.publish(file_hash, json.dumps({
            "type": "ingest",
            "content": "processing"
        }))

        host = os.getenv("MAEVE_HOST", "maeve:5000")
        url = f"http://{host}/documents/{document}"

        try:
            # TODO: better way to determine document loaders?
            if document.endswith(".pdf"):
                docs = PyMuPDFLoader(url).load()
            else:
                response = requests.get(url)
                with open(document, "wb") as f:
                    f.write(response.content)

                docs = TextLoader(document).load()
                os.remove(document)

            text_splitter = RecursiveCharacterTextSplitter(chunk_size=1024, chunk_overlap=100)
            chunks = text_splitter.split_documents(docs)

            # Add the file hash as metadata for each chunk
            [doc.metadata.update({"hash": file_hash}) for doc in chunks]

            redis.publish(file_hash, json.dumps({
                "type": "ingest",
                "content": "vectorizing"
            }))

            Chroma.from_documents(
                documents=chunks,
                collection_name="documents",
                ids=UUIDGenerator(len(chunks)),
                embedding=self.embeddings,
                client=self._get_chroma_client()
            )

            redis.publish(file_hash, json.dumps({
                "type": "ingest",
                "content": "completed"
            }))

        except Exception as e:
            redis.publish(file_hash, json.dumps({
                "type": "ingest",
                "content": "error"
            }))

            logger.error(f"Ingestion failed, {e}")

    def delete(self, file_hash: str):
        logger.info(f"Deleting file with hash {file_hash} from vector store")

        vector_store = Chroma(
            collection_name="documents",
            embedding_function=self.embeddings,
            client=self._get_chroma_client()
        )
        vector_store.delete(where={"hash": file_hash})

    def _get_chroma_client(self):
        chroma_host = os.getenv("CHROMA_HOST", "maeve-chroma")

        return HttpClient(
            host=chroma_host,
            port=8000,
            settings=Settings(anonymized_telemetry=False)
        )