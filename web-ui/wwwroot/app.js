window.onload = function () {
};

const apiUrl = 'http://localhost:8081/document';

// Fetch and display a document by its ID
async function getDocument(id) {
    const response = await fetch(`http://localhost:8081/Document/${id}`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

async function getDocuments(){
    const response = await fetch(`http://localhost:8081/Document`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

// Upload a document when the form is submitted
document.getElementById('uploadForm').addEventListener('submit', uploadForm);

async function uploadForm(event) {
    console.log(event);
    event.preventDefault(); // Prevent the default form submission

    const fileInput = document.getElementById('fileInput');
    const file = fileInput.files[0];

    let outputDiv = document.getElementById("output");

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
    console.log("Name is " + formData.Name);
    console.log("Filetype is " + formData.FileType);
    console.log("file size is " + formData.FileSize);

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
    }).then(response => {
        outputDiv.innerHTML = response.statusText;
    });
}