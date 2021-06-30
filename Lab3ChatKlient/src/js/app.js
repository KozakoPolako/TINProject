
import  * as signalR  from "@microsoft/signalr";


let username;
const selected = { item: "none" , type:"none"};
let activeUsers;
const loginBtn = document.querySelector(".loginBtn");
//zbior konwersacji
const messages = new Set(); 
//konwersacja 
class conversation{
    
    constructor(groupname, type) {
        this.groupName = groupname;
        this.type = type;
        this.content = new Set();
    }
    addMessage(message){
        this.content.add(message);
    }
}
//wiadomosc
class message{
    constructor(sender, time, message) {
        this.sender = sender;
        this.time = time;
        this.message = message;
    }
}




console.log(signalR);
var connection =connection = new signalR.HubConnectionBuilder()
.configureLogging(signalR.LogLevel.Debug)
.withUrl("http://localhost:5000/czathub", {
//.withUrl("http://192.168.0.246:5000/czathub", {
  skipNegotiation: true,
  transport: signalR.HttpTransportType.WebSockets
})
.build();


// wyłączenie przycisku do momentu połączenia 
loginBtn.disabled = true;




//start połączenia 
connection.start().then( () => {
    loginBtn.disabled = false;
    
    loginBtn.style.color = "white";
    loginBtn.value="Start";
}).catch ( err => {
    loginBtn.style.color = "red";
    loginBtn.value="Conn.ERROR";
    return console.error(err.toString());
});

loginBtn.addEventListener("click", (event) => {
    username = document.querySelector(".userNameInput").value;
    
    //console.log("Zalogowano");
    connection.invoke("Login",username, document.querySelector(".passwordInput").value)
        .catch(err => console.error(err.toString()));
    
    
    
    event.preventDefault();

    
    
})

// klawisz enter aktywuje przycisk Start
document.querySelector(".userNameInput").addEventListener("keyup", (event) => {

    if (event.key === 'Enter') {
        event.preventDefault();
        loginBtn.click();
    } 
})

connection.on("LoginStatus", function(status) {
    console.dir(status);
    if (status) {
        prepareWindow();
        connection.invoke("GetUsers")
            .catch(err => console.error(err.toString()));
        connection.invoke("getGroupsByUser",username)
            .catch(err => console.error(err.toString()));
    }else 
    {
        loginBtn.style.color = "red";
        loginBtn.value="ERROR";
        document.querySelector(".passwordInput").addEventListener("input", ()=> {
            loginBtn.style.color = "white";
            loginBtn.value="LOGIN";
        });
    }
});








// tworzenie okna po zalogowaniu  
const prepareWindow=function() {

    const children = Array.prototype.slice.call(loginBtn.parentNode.children);
    children.forEach(el => el.remove());

    const container = document.querySelector(".container");
    container.remove();

    const appPanel = document.createElement("div");
    
    const groupsList = document.createElement("div");
    const usersList = document.createElement("div");

    const messagesView = document.createElement("div");
    const messageInput = document.createElement("input");
    const sendMessage = document.createElement("div");
    const breakLine = document.createElement("div");
    const join = document.createElement("div");
    
    const div1 = document.createElement("div");
    const div2 = document.createElement("div");
    const div3 = document.createElement("div");

    div1.style.width="100%";
    div1.style.display="flex";
    div2.style.width="100%";

    appPanel.classList.add("appPanel");

    groupsList.classList.add("listPanel");
    groupsList.id = "groupsList";
    groupsList.style.height ="400px";

    usersList.classList.add("listPanel");
    usersList.id = "usersList";

    messagesView.classList.add("messagesView");
    messageInput.classList.add("messageInput");
    sendMessage.classList.add("sendMessage");
    breakLine.classList.add("break");
    join.classList.add("joinBTN")
    sendMessage.innerHTML = "<p>SEND</p>";
    join.innerHTML = "<p>JOIN</p>";
    
    
    
    container.appendChild(messagesView);
    container.appendChild(breakLine);
    container.appendChild(div1);
    div1.appendChild(div2);
    div2.appendChild(messageInput);
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

    connection.on("ReceiveMessage", function (destination, json) {
        const message = JSON.parse(json);
        console.dir(message);
        const msg = document.createElement("p");
        if (selected.item === destination && selected.type === "group"){
            messagesView.appendChild(msg);
            if(message.sender === username) {
                message.sender = "You";
                msg.style.width="80%";
                msg.style.float="right";
                msg.style.textAlign="right";
                msg.style.marginRight="3px";
                msg.style.color="#03A062";
            }else{
                msg.style.width="80%";
                msg.style.float="left";
                msg.style.textAlign="left";
                msg.style.marginLeft="3px";
                msg.style.color="white";
            }
        
        //skrolowanie listy wiadomości do dołu 
        messagesView.scrollTop = messagesView.scrollHeight;
        msg.innerHTML = `<span style="color: #00bfff;">${message.sender}:</span><span>${message.msg}</span>`;
        }
        
    });

    connection.on("ReceivePrivateMessage", function (destination, json) {
        const message = JSON.parse(json);
        console.dir(message);
        const msg = document.createElement("p");
        if (selected.item === destination && selected.type === "user"){
            messagesView.appendChild(msg);
            if(message.sender === username) {
                message.sender = "You";
                msg.style.width="80%";
                msg.style.float="right";
                msg.style.textAlign="right";
                msg.style.marginRight="3px";
                msg.style.color="#03A062";
            }else{
                msg.style.width="80%";
                msg.style.float="left";
                msg.style.textAlign="left";
                msg.style.marginLeft="3px";
                msg.style.color="white";
            }
        
        //skrolowanie listy wiadomości do dołu 
        messagesView.scrollTop = messagesView.scrollHeight;
        msg.innerHTML = `<span style="color: #00bfff;">${message.sender}:</span><span>${message.msg}</span>`;
        }
        
    });

    connection.on("ReciveUserList" ,  json => {
        //activeUsers  = JSON.parse(json);
        //console.dir(activeUsers);
        buildUsersList(json);
    } );

    connection.on("ReciveGroupList" ,  json => {
        //activeUsers  = JSON.parse(json);
        //console.dir(activeUsers);
        buildGroupsList(json);
    } );

    connection.on("ReciveMessagesByGroup" ,  json => {
        const conv = JSON.parse(json);
        console.dir(conv);
        messages.forEach(obj =>{
            if (obj.groupName === conv.groupName) messages.delete(obj);
        });

        messages.add(conv);

        showMessages(conv);
    } );

    

    join.addEventListener("click", addToGroup);
    
    document.querySelector(".sendMessage").addEventListener("click", (event) => {
        const message = document.querySelector(".messageInput").value;
        document.querySelector(".messageInput").value = "";
        if (selected.item ==="none"){
           connection.invoke("SendMessage", username, message).catch( (err) => console.error(err.toString()) );
           console.log("wiadomość do wszystkich"); 
        }
        else if(selected.type ==="user") {
            connection.invoke("SendPrivateMessage", username,selected.item, message).catch( (err) => console.error(err.toString()) );
            console.log("wiadomość prywatna");
            connection.invoke("SendPrivateMessage", username,username, message).catch( (err) => console.error(err.toString()) );
            console.log("wiadomość prywatna");
        }else {
            connection.invoke("SendMessageToGroup", username,selected.item, message).catch( (err) => console.error(err.toString()) );
            console.log("wiadomość do grupy");
        }
        
    });

    // klawisz enter aktywuje przycisk sendsendMessage
    messageInput.addEventListener("keyup", (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            sendMessage.click();
        }
    })
    // Testowanie Grup ();
    console.log("jo");
    //connection.invoke("AddUserToGroup","Czarek","Grupa nie testowa");
    //connection.invoke("AddUserToGroup","Marek","Grupa nie testowa");
    //connection.invoke("AddUserToGroup","Darek","prawidlowa");
    //connection.invoke("SendMessageToGroup","Darek","Grupa Testowa","Wiadomość testowa");
    
    
};

const buildUsersList = function(json) {

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
        if (el.isActive === "True" ) {
            user.style.color="#03A062";
        }else {
            user.style.color="white"; }
        
        
        if (el.name === selected.item) {
            user.classList.add("selected");
        }
        
        user.addEventListener("click",getSelected);
        usersList.appendChild(user);
    });



    setTimeout(() => {
        connection.invoke("GetUsers")
            .catch(err => console.error(err.toString()));
        connection.invoke("getGroupsByUser",username)
            .catch(err => console.error(err.toString()));
        console.log(username);
    },15000);
}

const buildGroupsList = function(json) {

    const usersList = document.querySelector("#groupsList");

    const children = Array.prototype.slice.call(usersList.children);
    children.forEach(el => el.remove());

    console.log("grupy");
    let group;
    const Groups = JSON.parse(json);
    console.dir(Groups);
    Groups.forEach(el => {
        group = document.createElement("p");
        group.innerHTML = el;
        group.style.color="white";
        if (el === selected.item) {
            group.classList.add("selected");
        }
        group.addEventListener("click",getSelected);
        usersList.appendChild(group);
        
    });
}

const getSelected = function() {

    document.querySelector(".joinBTN").innerHTML ="<p>JOIN</p>";

    //    
    //console.dir(this);
    const sel = document.querySelector(".selected");
    if ( sel ){
        sel.classList.remove("selected");
    }
    ;
    if (selected.item === this.innerHTML) {
        selected.item = "none";
        selected.type = "none";
    }else{

        selected.item =this.innerHTML;
        if(this.parentNode.id === "usersList") {
            buildConversation(selected.item,"private");
            selected.type = "user";
        }else {
            document.querySelector(".joinBTN").innerHTML ="<p>LEAVE</p>";
            buildConversation(selected.item,"group");
            selected.type = "group";
        }
        this.classList.add("selected");
    }
    console.dir(selected);



    
}

const addToGroup = function() {

    if(selected.type === "group"){

        connection.invoke("RemoveUserFromGroup",username,selected.item)
            .catch(err => console.error(err.toString()));
        connection.invoke("getGroupsByUser",username)
            .catch(err => console.error(err.toString()));
        selected.item ="none";
        selected.type ="none";
        document.querySelector(".joinBTN").innerHTML ="<p>JOIN</p>";

    }else {
        const window = document.createElement("div");
        const groupInput = document.createElement("input");
        const joinBTN = document.createElement("div");

        groupInput.classList.add("groupInput");
        joinBTN.classList.add("joinTogroupBTN");
        window.classList.add("addToGroup");

        groupInput.placeholder ="Groupname";
        joinBTN.innerHTML ="<p>EXIT</p>";

        window.appendChild(groupInput);
        window.appendChild(joinBTN);

        document.querySelector("body").appendChild(window);
        // zmiana wartości przycisku w zależności od wartości pola
        groupInput.addEventListener("input" , ()=>{
            if (groupInput.value ===""){
                joinBTN.innerHTML ="<p>EXIT</p>";
            }else {
                joinBTN.innerHTML ="<p>JOIN</p>";
            }
        
        })

        //dodawanie do grupy
        joinBTN.addEventListener("click", () => {
            if (groupInput.value ===""){
                window.remove();
            }else {
            
                connection.invoke("AddUserToGroup",username,groupInput.value)
                    .catch(err => console.error(err.toString()));
                connection.invoke("getGroupsByUser",username)
                    .catch(err => console.error(err.toString()));
                window.remove();
            }
        });
    }
    
}

const buildConversation = function(convName,type){

    
    connection.invoke("getMessagesByGroup",convName,type)
                    .catch(err => console.error(err.toString()));

    const messagesView = document.querySelector(".messagesView");
    const children = Array.prototype.slice.call(messagesView.children);
    children.forEach(el => el.remove());
}
const showMessages = function(conv){

    const messagesView = document.querySelector(".messagesView");
    
    conv.content.forEach(el =>{
        const msg = document.createElement("p");
        
        messagesView.appendChild(msg);
        if(el.sender === username) {
            user = "You";
            msg.style.width="80%";
            msg.style.float="right";
            msg.style.textAlign="right";
            msg.style.marginRight="3px";
            msg.style.color="#03A062";
        }else{
            msg.style.width="80%";
            msg.style.float="left";
            msg.style.textAlign="left";
            msg.style.marginLeft="3px";
            msg.style.color="white";
        }
        
        msg.innerHTML = `<span style="color: #00bfff;">${el.sender}:</span><span>${el.message}</span>`;
    });
    //skrolowanie listy wiadomości do dołu 
    messagesView.scrollTop = messagesView.scrollHeight;
    
    
    

}
