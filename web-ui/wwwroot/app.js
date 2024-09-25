const apiUrl = 'http://localhost:8080/todo';

// Function to fetch and display Todo items
function fetchTodoItems() {
    console.log('Fetching Todo items...');
    fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            const todoList = document.getElementById('todoList');
            todoList.innerHTML = ''; // Clear the list before appending new items
            data.forEach(task => {
                // Create list item with delete and toggle complete buttons
                const li = document.createElement('li');
                li.innerHTML = `
                    <span>Task: ${task.name} | Completed: ${task.isComplete}</span>
                    <button class="delete" style="margin-left: 10px;" onclick="deleteTask(${task.id})">Delete</button>
                    <button style="margin-left: 10px;" onclick="toggleComplete(${task.id}, ${task.isComplete}, '${task.name}')">
                        Mark as ${task.isComplete ? 'Incomplete' : 'Complete'}
                    </button>
                `;
                todoList.appendChild(li);
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der Todo-Items:', error));
}


// Function to add a new task
function addTask() {
    const taskName = document.getElementById('taskName').value;
    const isComplete = document.getElementById('isComplete').checked;

    if (taskName.trim() === '') {
        alert('Please enter a task name');
        return;
    }

    const newTask = {
        name: taskName,
        isComplete: isComplete
    };

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newTask)
    })
        .then(response => {
            if (response.ok) {
                fetchTodoItems(); // Refresh the list after adding
                document.getElementById('taskName').value = ''; // Clear the input field
                document.getElementById('isComplete').checked = false; // Reset checkbox
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => alert("Fehler: " + err.message));
                console.error('Fehler beim Hinzufügen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Function to delete a task
function deleteTask(id) {
    fetch(`${apiUrl}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                fetchTodoItems(); // Refresh the list after deletion
            } else {
                console.error('Fehler beim Löschen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}

// Function to toggle complete status
function toggleComplete(id, isComplete, name) {
    // Aufgabe mit umgekehrtem isComplete-Status aktualisieren
    const updatedTask = {
        id: id,  // Die ID des Tasks
        name: name, // Der Name des Tasks
        isComplete: !isComplete // Status umkehren
    };

    fetch(`${apiUrl}/${id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedTask)
    })
        .then(response => {
            if (response.ok) {
                fetchTodoItems(); // Liste nach dem Update aktualisieren
                console.log('Erfolgreich aktualisiert.');
            } else {
                console.error('Fehler beim Aktualisieren der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Load todo items on page load
document.addEventListener('DOMContentLoaded', fetchTodoItems);