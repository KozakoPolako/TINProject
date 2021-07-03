
import * as signalR from "@microsoft/signalr";
import { serverConn } from "./serverConn.js";


let username;
const server = new serverConn();

const base64 = { byte: "", type: "" };
const selected = { item: "none", type: "none" };
let activeUsers;
const loginBtn = document.querySelector(".loginBtn");

const unseen = new Set();
//zbior konwersacji
const messages = new Set();
//konwersacja 
class conversation {

    constructor(groupname, type) {
        this.groupName = groupname;
        this.type = type;
        this.content = new Set();
    }
    addMessage(message) {
        this.content.add(message);
    }
}
//wiadomosc
class message {
    constructor(sender, time, message) {
        this.sender = sender;
        this.time = time;
        this.message = message;
    }
}





//var connection = server.getConnection();


// wyłączenie przycisku do momentu połączenia 
loginBtn.disabled = true;

//nawiązywanie połączenia 
server.start(() => {
    loginBtn.disabled = false;
    console.log("Działam z obiektu");
    loginBtn.style.color = "white";
    loginBtn.value = "Start";
}, (err) => {
    loginBtn.style.color = "red";
    loginBtn.value = "Conn.ERROR";
    return console.error(err.toString());
});



loginBtn.addEventListener("click", (event) => {

    username = document.querySelector(".userNameInput").value;
    server.login(username, document.querySelector(".passwordInput").value);
    event.preventDefault();
});

// klawisz enter aktywuje przycisk Start
document.querySelector(".userNameInput").addEventListener("keyup", (event) => {

    if (event.key === 'Enter') {
        event.preventDefault();
        loginBtn.click();
    }
})

//nasłuchowanie statusu
server.onLoginStatus(function() {
    prepareWindow();
    server.getUsers();
    server.getGroupsByUser(username);
}, () => {
    loginBtn.style.color = "red";
    loginBtn.value = "ERROR";
    document.querySelector(".passwordInput").addEventListener("input", () => {
        loginBtn.style.color = "white";
        loginBtn.value = "LOGIN";
    });
})










// tworzenie okna po zalogowaniu  
const prepareWindow = function () {

    const children = Array.prototype.slice.call(loginBtn.parentNode.children);
    children.forEach(el => el.remove());

    const container = document.querySelector(".container");
    container.remove();

    const appPanel = document.createElement("div");

    const groupsList = document.createElement("div");
    const usersList = document.createElement("div");

    const messagesView = document.createElement("div");
    const messageInput = document.createElement("input");
    const imageInput = document.createElement("input");
    const sendMessage = document.createElement("div");
    const breakLine = document.createElement("div");
    const join = document.createElement("div");

    const div1 = document.createElement("div");
    const div2 = document.createElement("div");
    const div3 = document.createElement("div");
    const divBox = document.createElement("div");

    div1.style.width = "100%";
    div1.style.display = "flex";
    div2.style.width = "100%";

    appPanel.classList.add("appPanel");

    groupsList.classList.add("listPanel");
    groupsList.id = "groupsList";
    groupsList.style.height = "400px";

    usersList.classList.add("listPanel");
    usersList.id = "usersList";

    imageInput.classList.add("imageInput");
    messagesView.classList.add("messagesView");
    messageInput.classList.add("messageInput");
    sendMessage.classList.add("sendMessage");
    breakLine.classList.add("break");
    join.classList.add("joinBTN");
    divBox.classList.add("inputBox");


    sendMessage.innerHTML = "<p>SEND</p>";
    join.innerHTML = "<p>JOIN</p>";
    //divBox.innerHTML = "<p>M</p>";

    imageInput.type = "file";
    imageInput.accept = "image/*, audio/*";


    divBox.appendChild(imageInput);
    container.appendChild(messagesView);
    container.appendChild(breakLine);
    container.appendChild(div1);
    div1.appendChild(div2);
    div2.appendChild(messageInput);
    div1.appendChild(divBox);
    div1.appendChild(sendMessage);

    div3.appendChild(groupsList);
    div3.appendChild(join);

    appPanel.appendChild(div3);
    appPanel.appendChild(container);
    appPanel.appendChild(usersList);

    document.querySelector(".content").appendChild(appPanel);
    //container.appendChild(appPanel);
    //container.appendChild(messegeInput);
    //container.appendChild(sendMessege);

    imageInput.addEventListener("change", e => {
        const file = e.target.files[0];
        base64.type = file.type.substring(0, 5);
        console.log("===========================================================");
        console.log("Typ piku :", base64.type);
        console.log("===========================================================");
        toBase64(file);
    });

    server.onReceiveGroupMessage(appendMessage);

    server.onReceivePrivateMessage(appendMessage);   
    
    server.onReceiveUsersList(buildUsersList);
    
    server.onReceiveGroupsList(buildGroupsList);
    
    server.onReceiveMessagesPack( (json) => {
        const conv = JSON.parse(json);
        console.dir(conv);
        showMessages(conv);
    });
    



    join.addEventListener("click", addToGroup);

    document.querySelector(".sendMessage").addEventListener("click", (event) => {
        const message = document.querySelector(".messageInput").value;
        document.querySelector(".messageInput").value = "";
        if (document.querySelector(".imageInput").value != "") {

            if (selected.type === "user") {
                server.sendPrivateMessage(username, selected.item, base64.byte, base64.type);
                
            } else {

                server.sendGroupMessage(username, selected.item, base64.byte, base64.type);
                
            }
            document.querySelector(".imageInput").value = "";
        }
        else {
            if (selected.item === "none") {
                console.log("No destination selected");
                document.querySelector(".messageInput").value = message;
            }

            else if (selected.type === "user") {

                server.sendPrivateMessage(username, selected.item, message, "Text");
                
                console.log("wiadomość prywatna");
                
            } else {
                server.sendGroupMessage(username, selected.item, message, "Text");
                
                console.log("wiadomość do grupy");
            }
        }

    });

    // klawisz enter aktywuje przycisk sendsendMessage
    messageInput.addEventListener("keyup", (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            sendMessage.click();
        }
    });
    
};

const buildUsersList = function (json) {

    console.log("działam");
    const usersList = document.querySelector("#usersList");

    const children = Array.prototype.slice.call(usersList.children);
    children.forEach(el => el.remove());

    console.log("działam");
    let user;
    const Users = JSON.parse(json);
    console.dir(Users);
    Users.forEach(el => {
        user = document.createElement("p");
        user.innerHTML = el.name;
        user.id = el.name;
        if (unseen.has(el.name)) {
            user.classList.add("unseen");
        }
        if (el.isActive === "True") {
            user.style.color = "#03A062";
        } else {
            user.style.color = "white";
        }


        if (el.name === selected.item) {
            user.classList.add("selected");
        }

        user.addEventListener("click", getSelected);
        usersList.appendChild(user);
    });



    setTimeout(() => {
        server.getUsers();
        server.getGroupsByUser(username);
    }, 15000);
}

const buildGroupsList = function (json) {

    const usersList = document.querySelector("#groupsList");

    const children = Array.prototype.slice.call(usersList.children);
    children.forEach(el => el.remove());

    console.log("grupy");
    let flag = true;
    let group;
    const Groups = JSON.parse(json);
    console.dir(Groups);
    Groups.forEach(el => {
        group = document.createElement("p");
        group.innerHTML = el;
        console.log("Nazwa :::::" + el);
        group.id = el + "";
        group.style.color = "white";
        if (unseen.has(el)) {
            group.classList.add("unseen");
        }
        if (el === selected.item) {
            flag = false;
            group.classList.add("selected");
        }
        group.addEventListener("click", getSelected);
        usersList.appendChild(group);

    });
    console.log("flag", flag, " type", selected.type);
    if (flag === true && selected.type !== "user") {
        abortion();
    }
}

const getSelected = function () {

    document.querySelector(".joinBTN").innerHTML = "<p>JOIN</p>";

    if (selected.item !== "none") {
        console.log("działam tutaj przy zmianie");
        document.getElementById(selected.item).classList.remove("unseen");
        console.dir(document.getElementById(selected.item));
        unseen.delete(selected.item);
    }

    //console.dir(this);
    //this.classList.remove("unseen");

    const sel = document.querySelector(".selected");

    if (sel) {
        sel.classList.remove("selected");
    }
    ;
    if (selected.item === this.innerHTML) {

        this.color = "white";
        selected.item = "none";
        selected.type = "none";
    } else {

        selected.item = this.innerHTML;
        if (this.parentNode.id === "usersList") {
            buildConversation(selected.item, "Private");
            selected.type = "user";
        } else {
            document.querySelector(".joinBTN").innerHTML = "<p>LEAVE</p>";
            buildConversation(selected.item, "Public");
            selected.type = "group";
        }
        this.classList.add("selected");
    }
    console.dir(selected);




}

const addToGroup = function () {

    if (selected.type === "group") {

        server.removeUserFromGroup(username, selected.item);

        selected.item = "none";
        selected.type = "none";
        document.querySelector(".joinBTN").innerHTML = "<p>JOIN</p>";

    } else {
        const window = document.createElement("div");
        const groupInput = document.createElement("input");
        const joinBTN = document.createElement("div");

        groupInput.classList.add("groupInput");
        joinBTN.classList.add("joinTogroupBTN");
        window.classList.add("addToGroup");

        groupInput.placeholder = "Groupname";
        joinBTN.innerHTML = "<p>EXIT</p>";

        window.appendChild(groupInput);
        window.appendChild(joinBTN);

        document.querySelector("body").appendChild(window);
        // zmiana wartości przycisku w zależności od wartości pola
        groupInput.addEventListener("input", () => {
            if (groupInput.value === "") {
                joinBTN.innerHTML = "<p>EXIT</p>";
            } else {
                joinBTN.innerHTML = "<p>JOIN</p>";
            }

        })

        //dodawanie do grupy
        joinBTN.addEventListener("click", () => {
            if (groupInput.value === "") {
                window.remove();
            } else {

                server.addUserToGroup(username, groupInput.value);
                
                window.remove();
            }
        });
    }

}

const buildConversation = function (convName, type) {

    server.getMessagesByGroup(convName, type);
    
    abortion();
}
const appendMessage = function (destination, json, type) {
    const messagesView = document.querySelector(".messagesView");
    const message = JSON.parse(json);
    //console.dir(message);
    const msg = document.createElement("p");
    if (selected.item === destination && selected.type === type) {
        messagesView.appendChild(msg);
        if (message.sender === username) {
            message.sender = "You";
            msg.style.width = "80%";
            msg.style.float = "right";
            msg.style.textAlign = "right";
            msg.style.marginRight = "3px";
            msg.style.color = "#03A062";
        } else {
            msg.style.width = "80%";
            msg.style.float = "left";
            msg.style.textAlign = "left";
            msg.style.marginLeft = "3px";
            msg.style.color = "white";
        }

        //skrolowanie listy wiadomości do dołu 
        messagesView.scrollTop = messagesView.scrollHeight;

        if (message.type == "image") {
            msg.innerHTML = `<span style="color: #00bfff;">${message.sender}:</span><img src="${message.msg}"></img>`;
        } else if (message.type == "audio") {
            msg.innerHTML = `<span style="color: #00bfff;">${message.sender}:</span><audio controls ><source src="${message.msg}"></audio>`;
        } else {
            msg.innerHTML = `<span style="color: #00bfff;">${message.sender}:</span><span>${message.msg}</span>`;
        }
    } else {
        const name = document.getElementById(destination);
        name.classList.add("unseen");
        unseen.add(destination);

    }
}

const showMessages = function (conv) {
    console.log("Działam tutaj");
    const messagesView = document.querySelector(".messagesView");
    console.log("Działam tutaj");
    conv.content.forEach(el => {
        const msg = document.createElement("p");
        console.log("Działam tutaj");
        messagesView.appendChild(msg);
        console.log("Działam tutaj");
        if (el.sender === username) {
            console.log("Działam tutaj1");
            el.sender = "You";
            msg.style.width = "80%";
            msg.style.float = "right";
            msg.style.textAlign = "right";
            msg.style.marginRight = "3px";
            msg.style.color = "#03A062";
        } else {
            console.log("Działam tutaj2");
            msg.style.width = "80%";
            msg.style.float = "left";
            msg.style.textAlign = "left";
            msg.style.marginLeft = "3px";
            msg.style.color = "white";
        }
        console.log("Działam tutaj2 Type ", el.type);
        if (el.type == "image") {
            msg.innerHTML = `<span style="color: #00bfff;">${el.sender}:</span><img src="${el.msg}"></img>`;
        } else if (el.type == "audio") {
            msg.innerHTML = `<span style="color: #00bfff;">${el.sender}:</span><audio controls ><source src="${el.msg}"></audio>`;
        } else {
            msg.innerHTML = `<span style="color: #00bfff;">${el.sender}:</span><span>${el.msg}</span>`;
        }

    });
    //skrolowanie listy wiadomości do dołu 
    messagesView.scrollTop = messagesView.scrollHeight;




}
const abortion = function () {
    const messagesView = document.querySelector(".messagesView");
    const children = Array.prototype.slice.call(messagesView.children);
    children.forEach(el => el.remove());
}


const toBase64 = function (file) {

    //let base64 ;
    const reader = new FileReader();
    reader.onloadend = () => {
        // log to console
        // logs data:<type>;base64,wL2dvYWwgbW9yZ...
        base64.byte = reader.result;
        //console.log(reader.result);
    };
    console.log(base64.byte);
    reader.readAsDataURL(file);

}