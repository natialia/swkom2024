window.onload = function () {

};
async function getDocument(id) {
    const response = await fetch(`http://localhost:8081/Document/${id}`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

/*async function showHardcodedData() {
    const response = await fetch(`http://localhost:8081`);
    const data = await response.text();
    document.getElementById("output").textContent = data;
}

async function submitDocumentName() {
    let input = document.getElementById("docName");
    let outputDiv = document.getElementById("output");
    const name = input.textContent;
    input.textContent = "";

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
    public long Id { get; set; }
        public string? Name { get; set; }
        public bool? IsComplete { get; set; }
}*/