import os

from chromadb import Settings, HttpClient
from langchain_chroma import Chroma
from langchain_ollama import OllamaEmbeddings
from mcp.server import FastMCP

mcp = FastMCP("rag-mcp-server")

@mcp.tool("query_single_document", "Search in a document", "Search for relevant information in an already ingested document")
def query_document(filename: str, query: str) -> str:
    vector_store = _get_vector_store()
    retrieved_docs = vector_store.similarity_search(query=query, filter={"name": filename}, k=5)  # TODO: what is k
    if not retrieved_docs:
        return f"No information found in the document"

    return "\n\n".join(doc.page_content for doc in retrieved_docs)

@mcp.tool("query_all_documents", "Search in all documents", "Search for relevant information in all already ingested documents")
def query_all_documents(query: str) -> str:
    vector_store = _get_vector_store()

    retrieved_docs = vector_store.similarity_search(query=query, k=5) # TODO: what is k
    if not retrieved_docs:
        return f"No information found in any of the uploaded documents"

    return "\n\n".join(doc.page_content for doc in retrieved_docs)

def _get_vector_store() -> Chroma:
    embedding_model = os.getenv("OLLAMA_EMBEDDING_MODEL", "mxbai-embed-large")
    ollama_host = os.getenv("OLLAMA_HOST", "host.docker.internal")
    chroma_host = os.getenv("CHROMA_HOST", "maeve-chroma")

    return Chroma(
        collection_name="documents",
        embedding_function=OllamaEmbeddings(model=embedding_model, base_url=f"http://{ollama_host}:11434"),
        client=HttpClient(
            host=chroma_host,
            port=8000,
            settings=Settings(anonymized_telemetry=False)
        )
    )

if __name__ == "__main__":
    mcp.run(transport="stdio")
