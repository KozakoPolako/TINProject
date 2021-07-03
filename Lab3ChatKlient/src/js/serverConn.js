import  * as signalR  from "@microsoft/signalr";

class serverConn {

    constructor(){
        this.connection  = new signalR.HubConnectionBuilder()
            .configureLogging(signalR.LogLevel.Debug)
            .withUrl("http://localhost:5000/czathub", {
                //.withUrl("http://192.168.0.246:5000/czathub", {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();
    }

    /** 
     * Pobieranie połącenia w celach doagnostycznych 
     * 
     * @returns aktialne połączenie 
     */
    getConnection() { return this.connection }

    /** 
     * Rozpoczęcie połączenia wyklonuje metody zależenie od wyniku
     * 
     * @param {function} onSuccess  
     * @param {function} onError 
     */
    start(onSuccess, onError) {
        this.connection.start().then( () => {
            onSuccess();
        }).catch ( err => {
            onError(err);
        });
    }

    /** 
     * wysyła żądanie logowania
     * 
     * @param {string} username 
     * @param {string} password 
     */
    login(username ,password ) {
        this.connection.invoke("Login",username,password)
            .catch(err => console.error(err.toString()));
    }

    /**
     * Wwysyła żądanie pobrania listy użytkowników
     */
    getUsers() { 
        
        this.connection.invoke("GetUsers")
            .catch(err => console.error(err.toString()));
            
    }

    /**
     * Wysyła żądanie pobrania listy grup użytkownika
     */
    getGroupsByUser(username) {
        this.connection.invoke("getGroupsByUser",username)
            .catch(err => console.error(err.toString()));
    }

    
    /**
     * Włącza nasłuchwanie otrzymania statusu logowania
     * 
     * @param {function} onSuccess 
     * @param {function} onError 
     */
    onLoginStatus(onSuccess, onError) {
        this.connection.on("LoginStatus", function(status) {
            
            if (status) {
                onSuccess(); 
            }else 
            {
                onError();
            }
        });
    }

    /**
     * Włącza nasłuchiwanie wiadomości przychodzących od grup
     * 
     * @param {function} onEvent 
     */
    onReceiveGroupMessage(onEvent) { 
        this.connection.on("ReceiveMessage", function (destination, json) {
            
            onEvent(destination,json,"group");
        });
    }

    /**
     * Włącza nasłuchiwanie prywatnych wiadomośic 
     * 
     * @param {function} onEvent 
     */
    onReceivePrivateMessage(onEvent) { 
        this.connection.on("ReceivePrivateMessage", function (destination, json) {
            
            onEvent(destination,json,"user");
        });
    }

    /**
     * Włącza nasłuchiwanie przychodzącej listy użytkowników
     * 
     * @param {function} onEvent 
     */
    onReceiveUsersList(onEvent) {
        this.connection.on("ReceiveUserList", json => {
            onEvent(json);
        });
    }

    /**
     * Włącza nasłuchiwanie przychodzącej listy grup
     * 
     * @param {function} onEvent 
     */
    onReceiveGroupsList(onEvent) {
        this.connection.on("ReceiveGroupList", json => {
            onEvent(json);
        });
    }

    /**
     * Włącza nasłuchiwanie przychodzącej paczki wiadomości
     * 
     * @param {function} onEvent 
     */
    onReceiveMessagesPack(onEvent) {
        this.connection.on("ReceiveMessagesByGroup", json => {
            onEvent(json);
        });
    }

    /**
     * Wysyła wiadomość do grupy
     * 
     * @param {string} username 
     * @param {string} destination 
     * @param {string} message 
     * @param {string} type 
     */
    sendGroupMessage(username, destination, message, type){ 
        this.connection.invoke("SendMessageToGroup", username, destination, message, type)
            .catch((err) => console.error(err.toString()));
    }

    /**
     * wysyła wiadomość prywatną
     * 
     * @param {string} username 
     * @param {string} destination 
     * @param {string} message 
     * @param {string} type 
     */
    sendPrivateMessage(username, destination, message, type){
        this.connection.invoke("SendPrivateMessage", username, destination, message, type)
        .catch((err) => console.error(err.toString()));
    }
    /**
     * Wysyła żądanie usunięcia użytkownika z grupy
     * 
     * @param {string} username 
     * @param {string} group 
     */
    removeUserFromGroup(username, group){
        this.connection.invoke("RemoveUserFromGroup", username, group)
            .catch(err => console.error(err.toString())); 

    }

    /**
     * Wysyła żądanie dodania użytkownika do grupy
     * 
     * @param {string} username 
     * @param {string} group 
     */
    addUserToGroup(username, group){
        this.connection.invoke("AddUserToGroup", username, group)
            .catch(err => console.error(err.toString()));
    }

    /**
     * Wysyła żądanie pobrania paczki wiadomości z danej konwersacji
     * 
     * @param {string} convName 
     * @param {string} type 
     */
    getMessagesByGroup(convName,type) {
        this.connection.invoke("getMessagesByGroup", convName, type)
            .catch(err => console.error(err.toString()));
    }
    
}
export {serverConn};