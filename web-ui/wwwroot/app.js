window.onload = function () {
    const fileInput = document.getElementById('fileInput');

    // Display selected file information
    fileInput.addEventListener('change', function () {
        const file = fileInput.files[0];
        if (file) {
            const fileInfo = `Selected file: ${file.name} (${(file.size / 1024).toFixed(2)} kB)`;
            document.getElementById("selectedFileInfo").textContent = fileInfo; // Display file name and size
        } else {
            document.getElementById("selectedFileInfo").textContent = ''; // Clear info if no file is selected
        }
    });
};

const apiUrl = 'http://localhost:8081/document';

// Fetch and display a document by its ID
async function getDocument(id) {
    const response = await fetch(`http://localhost:8081/Document/${id}`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

async function getDocuments() {
    const response = await fetch(`http://localhost:8081/Document`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

// Upload a document when the form is submitted
document.getElementById('uploadForm').addEventListener('submit', uploadForm);

async function uploadForm(event) {
    event.preventDefault(); // Prevent the default form submission

    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];
    let outputDiv = document.getElementById("output");

    if (!file) {
        alert('Please select a file to upload!');
        return;
    }

    const formData = new FormData();
    formData.append('Id', 0); // Placeholder ID
    formData.append('Name', file.name.substring(0, file.name.lastIndexOf('.')));
    formData.append('FileType', file.type);
    formData.append('FileSize', `${Math.round(file.size / 1024)} kB`);
    formData.append('file', file); // Add the file to FormData

    fetch(`${apiUrl}/upload`, {
        method: 'POST',
        body: formData // Send FormData
    }).then(response => {
        if (response.ok) {
            outputDiv.innerHTML = 'File uploaded successfully.';
            getDocuments(); // Refresh document list after upload
        } else {
            outputDiv.innerHTML = 'Failed to upload file.';
        }
    }).catch(error => {
        console.error('Error:', error);
        outputDiv.innerHTML = 'Error occurred during upload.';
    });
}
