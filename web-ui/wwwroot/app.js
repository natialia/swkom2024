window.onload = function () {
    showHardcodedData();
};
async function getDocument(id) {
    const response = await fetch(`http://localhost:8081/Document/${id}`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

async function showHardcodedData() {
    const response = await fetch(`http://localhost:8081`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}