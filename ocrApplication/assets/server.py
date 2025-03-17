import sys
from flask import Flask, request, jsonify
import ollama

app = Flask(__name__)

@app.route('/process_ocr', methods=['POST'])
def process_ocr():
    # Step 1: Parse the JSON body containing OCR extracted text
    data = request.get_json()

    # Check if the 'texts' key exists in the request body
    if 'texts' not in data or not isinstance(data['texts'], list):
        return jsonify({"error": "Invalid input. Please provide a list of OCR-extracted texts."}), 400
    
    extracted_text = data['texts']  # Extract OCR text from the request body
    
    # Step 2: Process text with DeepSeek-R1 model using ollama
    try:
        response = ollama.chat(
            model="llava",
            messages=[
                {
                    "role": "system",
                    "content": (
                        "You are an expert text processor tasked with acquiring multiple OCR-extracted texts "
                        "from the same image into a single, accurate ground truth text using a majority voting technique. "
                        "Align the input strings, accounting for differences in length and word order. For each word position, "
                        "select the most common word across all inputs, defaulting to the most reliable input if there's no clear majority."
                    ),
                },
                {
                    "role": "user",
                    "content": f"The list of OCR extracted text is {extracted_text}",
                },
            ],
        )
        
        # Step 3: Return the response from the model
        return jsonify({
            'processed_text': response['message']['content']
        })
    
    except Exception as e:
        return jsonify({"error": f"An error occurred while processing: {str(e)}"}), 500


if __name__ == '__main__':
    app.run(debug=True)
