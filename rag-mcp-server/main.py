import os

import requests
from chromadb import Settings
from chromadbx import UUIDGenerator
from langchain_chroma import Chroma
from langchain_community.document_loaders import PyMuPDFLoader, TextLoader
from langchain_community.vectorstores.utils import filter_complex_metadata
from langchain_ollama import OllamaEmbeddings
from langchain_text_splitters import RecursiveCharacterTextSplitter
from mcp.server import FastMCP

mcp = FastMCP("rag-agent")

# @mcp.tool("ingest", "ingest", "Vectorize a document")
# def ingest(document: str, file_hash: str, embedding_model: str) -> str:
#     try:
#         path = f"http://localhost:5284/documents/{document}"
#
#         if document.endswith(".pdf"):
#             docs = PyMuPDFLoader(path).load()
#         else:
#             response = requests.get(path)
#             with open(document, "wb") as f:
#                 f.write(response.content)
#
#             docs = TextLoader(document).load()
#             os.remove(document)
#
#         text_splitter = RecursiveCharacterTextSplitter(chunk_size=1024, chunk_overlap=100)
#         chunks = text_splitter.split_documents(docs)
#         chunks = filter_complex_metadata(chunks)
#         [doc.metadata.update({"hash": file_hash}) for doc in docs]
#
#         Chroma.from_documents(
#             documents=chunks,
#             collection_name="documents",
#             ids=UUIDGenerator(len(chunks)),
#             embedding=OllamaEmbeddings(model=embedding_model, base_url="http://localhost:11434"),
#             persist_directory="chroma_db",
#             client_settings=Settings(anonymized_telemetry=False)
#         )
#
#         return f"Document {document} ingested successfully"
#     except Exception as e:
#         return f"Ingestion failed, {e}"

@mcp.tool("query", "Search in documents", "Search for relevant information in all already ingested documents")
def query(query: str) -> str:
    host = os.getenv("OLLAMA_HOST", "localhost")

    vector_store = Chroma(
        collection_name="documents",
        embedding_function=OllamaEmbeddings(model="mxbai-embed-large:latest", base_url=f"http://{host}:11434"),
        persist_directory="chroma_db",
        client_settings=Settings(anonymized_telemetry=False)
    )

    retrieved_docs = vector_store.similarity_search(query=query, k=5) # TODO: what is k
    if not retrieved_docs:
        return f"No information found in any of the uploaded documents"

    return "\n\n".join(doc.page_content for doc in retrieved_docs)

if __name__ == "__main__":
    mcp.run(transport="stdio")