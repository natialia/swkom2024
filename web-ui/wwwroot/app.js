﻿window.onload = function () {
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

    const formData = {
        Id: 0,
        Name: file.name
    };

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