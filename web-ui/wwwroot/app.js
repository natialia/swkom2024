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

    documents.forEach(doc => {
        const listItem = document.createElement('div');
        listItem.classList.add('document-item'); // Add a class for styling

        // Create a formatted display
        listItem.innerHTML = `
                        <strong>${doc.name}</strong><br>
                        <span class="file-info">Type: ${doc.fileType}</span>
                        <span class="file-info">Size: ${doc.fileSize}</span>
                        <button class="delete-button" onclick="deleteDocument(${doc.id})">Delete</button>
                    `;

        outputDiv.appendChild(listItem);
    });
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

async function uploadForm(event) {
    event.preventDefault(); // Prevent the default form submission

    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];

    let outputDiv = document.getElementById("response");

    if (!file) {
        alert('Please select a file to upload!');
        return;
    }

    let fileSize = file.size.toString();
    let fileString = `${Math.round(+fileSize / 1024).toFixed(2)}kB`;

    const formData = {
        Id: 0,
        Name: file.name.substring(0, file.name.lastIndexOf('.')),
        FileType: file.type,
        FileSize: fileString
    };

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
    }).then(response => {
        outputDiv.innerHTML = response.statusText;
        getDocuments(); // Refresh the document list after upload
    });
}