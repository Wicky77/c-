let items = [
	"Сделать проектную работу",
	"Полить цветы",
	"Пройти туториал по Реакту",
	"Сделать фронт для своего проекта",
	"Прогуляться по улице в солнечный день",
	"Помыть посуду",
];

const listElement = document.querySelector(".to-do__list");
const formElement = document.querySelector(".to-do__form");
const inputElement = document.querySelector(".to-do__input");

function loadTasks() {
	const savedTasks = localStorage.getItem("tasks");
	if (savedTasks) {
		return JSON.parse(savedTasks);
	}
	return items;
}

function createItem(item) {
	const template = document.getElementById("to-do__item-template");
	const clone = template.content.querySelector(".to-do__item").cloneNode(true);
	const textElement = clone.querySelector(".to-do__item-text");
	const deleteButton = clone.querySelector(".to-do__item-button_type_delete");
	const duplicateButton = clone.querySelector(".to-do__item-button_type_duplicate");
	const editButton = clone.querySelector(".to-do__item-button_type_edit");

	textElement.textContent = item;

	editButton.addEventListener("click", () => {
		textElement.setAttribute("contenteditable", "true");
		textElement.focus();
	});

	textElement.addEventListener("blur", () => {
		textElement.setAttribute("contenteditable", "false");
		const updatedItems = getTasksFromDOM();
		saveTasks(updatedItems);
	});

	deleteButton.addEventListener("click", () => {
		clone.remove();
		const updatedItems = getTasksFromDOM();
		saveTasks(updatedItems);
	});

	duplicateButton.addEventListener("click", () => {
		const newItem = createItem(textElement.textContent);
		listElement.prepend(newItem);
		const updatedItems = getTasksFromDOM();
		saveTasks(updatedItems);
	});

	return clone;
}

function getTasksFromDOM() {
	const itemsNamesElements = listElement.querySelectorAll(".to-do__item-text");
	const tasks = [];
	itemsNamesElements.forEach(element => {
		tasks.push(element.textContent);
	});
	return tasks;
}

function saveTasks(tasks) {
	localStorage.setItem("tasks", JSON.stringify(tasks));
}

items = loadTasks();
items.forEach(item => {
	listElement.append(createItem(item));
});

formElement.addEventListener("submit", (evt) => {
	evt.preventDefault();
	const taskText = inputElement.value.trim();
	if (taskText === "") {
		return;
	}

	const newItem = createItem(taskText);
	listElement.prepend(newItem);

	items = getTasksFromDOM();
	saveTasks(items);

	inputElement.value = "";
});
