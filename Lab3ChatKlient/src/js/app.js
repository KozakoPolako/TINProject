
import  * as signalR  from "@microsoft/signalr";


let username;
const loginBtn = document.querySelector(".loginBtn");

//var connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:5001/czatHub").build();
console.log(signalR);
var connection =connection = new signalR.HubConnectionBuilder()
.configureLogging(signalR.LogLevel.Debug)
.withUrl("http://localhost:5000/czathub", {
  skipNegotiation: true,
  transport: signalR.HttpTransportType.WebSockets
})
.build();

loginBtn.disabled = true;
//loginBtn.style.color = "red";

connection.on("ReceiveMessage", function (user, message) {
    // var li = document.createElement("li");
    // document.getElementById("messagesList").appendChild(li);
    // // We can assign user-supplied strings to an element's textContent because it
    // // is not interpreted as markup. If you're assigning in any other way, you 
    // // should be aware of possible script injection concerns.
    // li.textContent = `${user} says ${message}`;
});

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
    prepareWindow();
    console.log("Zalogowano");
    connection.invoke("Login",username)
        .catch(err => console.error(err.toString()));
    event.preventDefault();
    
})



const prepareWindow=function() {

    const children = Array.prototype.slice.call(loginBtn.parentNode.children);
    children.forEach(el => el.remove());

    const container = document.querySelector(".container");

    const messegesView = document.createElement("div");
    const messegeInput = document.createElement("input");
    const sendMessege = document.createElement("div");
    const breakLine = document.createElement("div");

    messegesView.classList.add("messegesView");
    messegeInput.classList.add("messegeInput");
    sendMessege.classList.add("sendMessege");
    breakLine.classList.add("break");
    sendMessege.innerHTML = "<p>SEND</p>";
    
    //container.style +="display: flex;";

    container.appendChild(messegesView);
    container.appendChild(breakLine);
    container.appendChild(messegeInput);
    container.appendChild(sendMessege);

    connection.on("ReceiveMessage", function (user, message) {
        const msg = document.createElement("p");
        document.querySelector(".messegesView").appendChild(msg);
        if(user === username) {
            msg.style.textAlign="right";
            msg.style.marginRight="3px";
            msg.style.color="#03A062";
        }else{
            msg.style.textAlign="left";
            msg.style.marginLeft="3px";
            msg.style.color="white";
        }
        

        
        
        msg.innerHTML = `${user}: ${message}`;
    });


    document.querySelector(".sendMessege").addEventListener("click", (event) => {
        const message = document.querySelector(".messegeInput").value;
        document.querySelector(".messegeInput").value = "";
        connection.invoke("SendMessage", username, message).catch( (err) => console.error(err.toString()) );
    });
       
    
};
