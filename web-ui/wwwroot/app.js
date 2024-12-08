const apiUrl = 'http://localhost:8081/document';

// Fetch and display documents
async function getDocuments() {
    const response = await fetch(`${apiUrl}`);
    const documents = await response.json(); // Get the JSON response
    displayDocuments(documents); // Display documents
}

// Display the list of documents
function displayDocuments(documents) {
    const outputDiv = document.getElementById("output");
    outputDiv.innerHTML = ''; // Clear existing list

    // Show a message if no documents are found
    if (documents.length === 0) {
        outputDiv.innerHTML = `<span class="error">No documents found.</span>`;
        return;
    }

    // Display each document
    documents.forEach(doc => {
        const listItem = document.createElement('div');
        listItem.classList.add('document-item');

        // Create a formatted display
        listItem.innerHTML = `
            <strong>${doc.name}</strong><br>
            <span class="file-info">Type: ${doc.fileType}</span>
            <span class="file-info">Size: ${doc.fileSize}</span>
        `;

        // Add Delete button
        const deleteButton = document.createElement('button');
        deleteButton.textContent = 'Delete';
        deleteButton.classList.add('delete-button');
        deleteButton.onclick = () => deleteDocument(doc.id);

        // Add Show Text button
        const textButton = document.createElement('button');
        textButton.textContent = 'Show Text';
        textButton.classList.add('delete-button');
        textButton.onclick = () => fetchDocumentText(doc.id, listItem);

        // Append buttons to the list item
        listItem.appendChild(deleteButton);
        listItem.appendChild(textButton);

        // Append the list item to the output
        outputDiv.appendChild(listItem);
    });
}


async function fetchDocumentText(id, listItem) {
    try {
        //check if text is already shown
        let existingTextElement = listItem.querySelector('.ocr-text');
        if (existingTextElement) {

            existingTextElement.remove();
            return;//text already exists: remo´ve it
        }

        //no text yet: get ocr text from apu
        const response = await fetch(`${apiUrl}/ocrText/${id}`);
        if (response.ok) {
            const ocrText = await response.text(); //get ocr text
            const textElement = document.createElement('p');
            textElement.textContent = `OCR Text: ${ocrText}`;
            textElement.classList.add('ocr-text'); //class to identify
            textElement.style.marginTop = '10px';
            textElement.style.color = '#333';
            listItem.appendChild(textElement);
        } else {
            alert('Failed to fetch OCR text.');
        }
    } catch (error) {
        console.error('Error fetching OCR text:', error);
        alert('An error occurred while fetching OCR text.');
    }
}

// Delete a document by its ID
async function deleteDocument(id) {
    const response = await fetch(`${apiUrl}/${id}`, {
        method: 'DELETE'
    });

    if (response.ok) {
        getDocuments(); // Refresh the document list after deletion
    } else {
        alert('Failed to delete the document.');
    }
}


// Upload a document when the form is submitted
document.getElementById('uploadForm').addEventListener('submit', uploadForm);

document.addEventListener('DOMContentLoaded', () => {
    const uploadForm = document.getElementById('uploadForm');
    const fileInput = document.getElementById('fileInput');
    const responseDiv = document.getElementById('response');

    // Attach the submit event listener
    uploadForm.addEventListener('submit', async (event) => {
        event.preventDefault(); // Prevent default form submission
        responseDiv.innerHTML = ''; // Clear previous messages

        const file = fileInput.files[0];

        // Handle no file selection
        if (!file) {
            responseDiv.innerHTML = `<span class="error">Please select a file to upload!</span>`;
            return;
        }

        // Prepare file data for upload
        const fileSize = `${Math.round(file.size / 1024).toFixed(2)}kB`;
        const fileName = file.name.substring(0, file.name.lastIndexOf('.'));

        const formData = new FormData();
        formData.append('UploadedDocument', file);
        formData.append('Id', 0);
        formData.append('Name', fileName);
        formData.append('FileType', file.type);
        formData.append('FileSize', fileSize);

        try {
            const response = await fetch('http://localhost:8081/document', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                responseDiv.innerHTML = `<span class="success">File uploaded successfully!</span>`;
                getDocuments(); // Refresh the document list
            } else {
                responseDiv.innerHTML = `<span class="error">File upload failed. Please try again.</span>`;
            }
        } catch (error) {
            console.error('Error:', error);
            responseDiv.innerHTML = `<span class="error">An error occurred while uploading the file.</span>`;
        }
    });
});