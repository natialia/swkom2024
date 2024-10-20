window.onload = function () {
};

// Fetch and display a document by its ID
async function getDocument(id) {
    const response = await fetch(`http://localhost:8081/Document/${id}`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

// Upload a document when the form is submitted
document.getElementById('uploadForm').addEventListener('submit', async (event) => {
    event.preventDefault(); // Prevent the default form submission

    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];

    if (!file) {
        alert('Please select a file to upload!');
        return;
    }

    const formData = new FormData();
    formData.append('document', file); // Append the file to FormData

    try {
        const response = await fetch('http://localhost:8081/api/documents/upload', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Error uploading document');
        }

        const result = await response.json();
        document.getElementById('response').innerText = `Document successfully uploaded: ${result.name}`;
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('response').innerText = `Error: ${error.message}`;
    }
});

// Optional: If you want to keep the previous document name submission function
async function submitDocumentName() {
    let input = document.getElementById("docName");
    let outputDiv = document.getElementById("output");
    const name = input.value; // Use value instead of textContent
    input.value = ""; // Clear the input

    const document = {
        name: name,
        isComplete: true
    };

    fetch("http://localhost:8081/Document", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(document)
    }).then(response => {
        outputDiv.innerHTML = response.statusText;
    });
}
