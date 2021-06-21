
import  * as signalR  from "@microsoft/signalr";


let username;
const loginBtn = document.querySelector(".loginBtn");


console.log(signalR);
var connection =connection = new signalR.HubConnectionBuilder()
.configureLogging(signalR.LogLevel.Debug)
.withUrl("http://localhost:5000/czathub", {
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
    prepareWindow();
    console.log("Zalogowano");
    connection.invoke("Login",username)
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


// tworzenie okna po zalogowaniu  
const prepareWindow=function() {

    const children = Array.prototype.slice.call(loginBtn.parentNode.children);
    children.forEach(el => el.remove());

    const container = document.querySelector(".container");

    const messegesView = document.createElement("div");
    const messegeInput = document.createElement("input");
    const sendMessege = document.createElement("div");
    const breakLine = document.createElement("div");
    
    const div1 = document.createElement("div");
    const div2 = document.createElement("div");

    div1.style.width="100%";
    div1.style.display="flex";
    div2.style.width="100%";

    messegesView.classList.add("messegesView");
    messegeInput.classList.add("messegeInput");
    sendMessege.classList.add("sendMessege");
    breakLine.classList.add("break");
    sendMessege.innerHTML = "<p>SEND</p>";
    

    container.appendChild(messegesView);
    container.appendChild(breakLine);
    container.appendChild(div1);
    div1.appendChild(div2);
    div2.appendChild(messegeInput);
    div1.appendChild(sendMessege);
    //container.appendChild(messegeInput);
    //container.appendChild(sendMessege);

    connection.on("ReceiveMessage", function (user, message) {
        const msg = document.createElement("p");
        
        messegesView.appendChild(msg);
        if(user === username) {
            user = "You";
            msg.style.textAlign="right";
            msg.style.marginRight="3px";
            msg.style.color="#03A062";
        }else{
            msg.style.textAlign="left";
            msg.style.marginLeft="3px";
            msg.style.color="white";
        }
        
        //skrolowanie listy wiadomości do dołu 
        messegesView.scrollTop = messegesView.scrollHeight;
        msg.innerHTML = `<span style="color: #00bfff;">${user}:</span><span>${message}</span>`;
    });


    document.querySelector(".sendMessege").addEventListener("click", (event) => {
        const message = document.querySelector(".messegeInput").value;
        document.querySelector(".messegeInput").value = "";
        connection.invoke("SendMessage", username, message).catch( (err) => console.error(err.toString()) );
    });

    // klawisz enter aktywuje przycisk sendsendMessage
    messegeInput.addEventListener("keyup", (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            sendMessege.click();
        }
    })
    
       
    
};
