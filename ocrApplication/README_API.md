# OCR Processing API Server

## Overview
The OCR Processing API Server is a Python-based service that enhances OCR results using AI models. It provides an HTTP endpoint for processing OCR-extracted text and improving accuracy through advanced text processing techniques.

## Features
- REST API for OCR text processing
- Integration with LLM models (LLaVA) via Ollama
- Majority voting technique for combining multiple OCR results
- Alignment of OCR-extracted texts to create a single accurate output
- Error handling and robust API responses

## API Endpoints

### POST /process_ocr
Processes multiple OCR-extracted texts to generate a single, accurate result.

**Request Format:**
```json
{
  "texts": [
    "First OCR extracted text",
    "Second OCR extracted text",
    "Third OCR extracted text"
  ]
}
```

**Response Format:**
```json
{
  "processed_text": "Accurate combined text result"
}
```

**Error Response:**
```json
{
  "error": "Error message"
}
```

## Architecture
The server is built using:
- Flask for the web server and REST API
- Ollama for local LLM inference
- LLaVA model for intelligent text processing

## Dependencies
- Python 3.x
- Flask
- Ollama Python client
- LLaVA model (locally hosted via Ollama)

## Setup & Installation
1. Install Python 3.x
2. Install required packages: `pip install flask ollama`
3. Set up Ollama with LLaVA model: `ollama pull llava`
4. Run the server: `python server.py`

## Configuration
The server runs on the default Flask port (5000) and can be configured for production deployment by modifying the appropriate settings.

## Integration with OCR Application
This API server complements the OCR Application by:
1. Receiving OCR texts extracted by the main application
2. Processing these texts with advanced AI techniques
3. Returning improved results to be used in the main application

## Security Considerations
For production use, consider:
- Adding authentication mechanisms
- Enabling HTTPS
- Rate limiting
- Input validation and sanitization

## Extending the API
To add new processing capabilities:
1. Create new route handlers in `server.py`
2. Implement additional processing logic
3. Update the API documentation accordingly 