import os

from chromadb import Settings, HttpClient
from langchain_chroma import Chroma
from langchain_ollama import OllamaEmbeddings
from mcp.server import FastMCP

mcp = FastMCP("rag-mcp-server")

@mcp.tool("query", "Search in documents", "Search for relevant information in all already ingested documents")
def query(query: str) -> str:
    embedding_model = os.getenv("OLLAMA_EMBEDDING_MODEL", "mxbai-embed-large")
    ollama_host = os.getenv("OLLAMA_HOST", "host.docker.internal")
    chroma_host = os.getenv("CHROMA_HOST", "maeve-chroma")

    vector_store = Chroma(
        collection_name="documents",
        embedding_function=OllamaEmbeddings(model=embedding_model, base_url=f"http://{ollama_host}:11434"),
        client=HttpClient(
            host=chroma_host,
            port=8000,
            settings=Settings(anonymized_telemetry=False)
        )
    )

    retrieved_docs = vector_store.similarity_search(query=query, k=5) # TODO: what is k
    if not retrieved_docs:
        return f"No information found in any of the uploaded documents"

    return "\n\n".join(doc.page_content for doc in retrieved_docs)

if __name__ == "__main__":
    mcp.run(transport="stdio")