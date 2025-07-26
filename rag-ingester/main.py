from dotenv import load_dotenv
from fastapi import FastAPI, BackgroundTasks

from dto import IngestRequest, DeleteRequest
from ingester import Ingester

load_dotenv()

app = FastAPI()
ingester = Ingester()

@app.post("/ingest")
def ingest(request: IngestRequest, background_tasks: BackgroundTasks):
    background_tasks.add_task(ingester.ingest, request.document, request.file_hash)

@app.delete("/document")
def delete_document(request: DeleteRequest, background_tasks: BackgroundTasks):
    background_tasks.add_task(ingester.delete, request.file_hash)