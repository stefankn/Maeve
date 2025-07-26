from pydantic import BaseModel

class IngestRequest(BaseModel):
    document: str
    file_hash: str

class DeleteRequest(BaseModel):
    file_hash: str