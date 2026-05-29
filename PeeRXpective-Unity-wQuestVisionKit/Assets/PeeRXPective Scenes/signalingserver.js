// signaling-server.js
const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8080 });

let clients = {}; // { clientId: ws }

wss.on('connection', (ws) => {
    let clientId = null;

    ws.on('message', (msg) => {
        const messageString = msg.toString();

        // Log a snippet of every message to see the traffic flow
        console.log("RAW MESSAGE:", messageString.substring(0, 60));

        // 1. Handle SimpleWebRTC Raw String Registration (NEWPEER|ID|...)
        if (messageString.startsWith("NEWPEER")) {
            const parts = messageString.split('|');
            clientId = parts[1]; // Extracts the PeerID (e.g., JNIbu-PeerId)
            clients[clientId] = ws;

            console.log(`[REGISTER] Peer Registered: ${clientId}`);

            // BROADCAST: Tell all OTHER connected clients that this peer has arrived
            wss.clients.forEach((client) => {
                if (client !== ws && client.readyState === WebSocket.OPEN) {
                    client.send(messageString);
                    console.log(`[BROADCAST] Sent NEWPEER ${clientId} to a neighbor.`);
                }
            });
            return;
        }

        // 2. Handle WebRTC Signaling (JSON Offers/Answers/Candidates/ACKs)
        // SimpleWebRTC often sends pipe-delimited strings for ACKs and DATA too
        if (messageString.includes('|')) {
            const parts = messageString.split('|');
            const type = parts[0];
            const senderId = parts[1];
            const targetId = parts[2];

            // If the target is a specific ID, forward it directly
            if (targetId && targetId !== "ALL" && clients[targetId]) {
                if (clients[targetId].readyState === WebSocket.OPEN) {
                    clients[targetId].send(messageString);
                    console.log(`[FORWARD] ${type} from ${senderId} -> ${targetId}`);
                }
            }
            // If the target is "ALL", broadcast it (used for NEWPEERACK)
            else if (targetId === "ALL") {
                wss.clients.forEach((client) => {
                    if (client !== ws && client.readyState === WebSocket.OPEN) {
                        client.send(messageString);
                    }
                });
                console.log(`[BROADCAST] ${type} from ${senderId} to ALL`);
            }
            return;
        }

        // 3. Fallback for pure JSON signaling (if your version uses it)
        try {
            const data = JSON.parse(messageString);
            const targetId = data.to || data.receiverPeerId;

            if (targetId && clients[targetId] && clients[targetId].readyState === WebSocket.OPEN) {
                clients[targetId].send(messageString);
                console.log(`[JSON] Forwarded ${data.type || 'msg'} to ${targetId}`);
            }
        } catch (e) {
            // Silently catch if it's just a test string or malformed
        }
    });

    ws.on('close', () => {
        // If we have the ID saved locally, use it
        if (clientId) {
            console.log(`[DISCONNECT] Peer Left: ${clientId}`);
            delete clients[clientId];
        } else {
            // Fallback: Find which ID was associated with this specific socket
            const disconnectedId = Object.keys(clients).find(key => clients[key] === ws);
            if (disconnectedId) {
                console.log(`[DISCONNECT] Peer Left (via lookup): ${disconnectedId}`);
                delete clients[disconnectedId];
            } else {
                console.log(`[DISCONNECT] An unregistered client disconnected.`);
            }
        }
    });

    ws.on('error', (err) => {
        console.error(`[SERVER ERROR] ${err.message}`);
    });
});

console.log("Signaling server running on ws://0.0.0.0:8080");


//// signaling-server.js
//const WebSocket = require('ws');

//const wss = new WebSocket.Server({ port: 8080 });

//let clients = {}; // { clientId: ws }

//wss.on('connection', (ws) => {
//    let clientId = null;

//    ws.on('message', (msg) => {

//        console.log("RAW MESSAGE RECEIVED:", msg.toString().substring(0, 50));

//        const messageString = msg.toString();

//        // 1. Handle SimpleWebRTC Raw String Registration (NEWPEER|ID|...)
//        if (messageString.startsWith("NEWPEER")) {
//            const parts = messageString.split('|');
//            clientId = parts[1]; // Extracts the PeerID (e.g., JNIbu-PeerId)
//            clients[clientId] = ws;

//            console.log(`Peer Registered: ${clientId}`);

//            // Optional: Broadcast to others that a new peer joined
//            // SimpleWebRTC usually handles peer discovery itself once registered
//            return;
//        }

//        // 2. Handle WebRTC Signaling (JSON Offers/Answers/Candidates)
//        try {
//            const data = JSON.parse(messageString);

//            // If we didn't get a clientId from NEWPEER yet, try to get it from JSON
//            if (!clientId && data.from) {
//                clientId = data.from;
//                clients[clientId] = ws;
//            }

//            const targetId = data.to;
//            if (targetId && clients[targetId] && clients[targetId].readyState === WebSocket.OPEN) {
//                clients[targetId].send(messageString);
//                console.log(`Forwarded ${data.type || 'message'} from ${clientId} to ${targetId}`);
//            }
//        } catch (e) {
//            console.log(`Received non-JSON/Unknown format: ${messageString}`);
//        }
//    });

//    ws.on('close', () => {
//        if (clientId) {
//            console.log(`Peer Disconnected: ${clientId}`);
//            delete clients[clientId];
//        }
//    });
//});

//console.log("Signaling server running on ws://0.0.0.0:8080");

//////////////////////////////////////////////////////////////

////// signaling-server.js//
//const WebSocket = require('ws');//

//const wss = new WebSocket.Server({ port: 8080 }//);

//let clients = {}; // { clientId: //ws }

//wss.on('connection',// ws => {
//    let clientId// = null;

//    ws.on('messag//e', msg => {
//        // First message i//s registration
//        //if (!clientId) {
//            clientId = JSON.parse(m//sg); // e.g., "T1"
//            cli//ents[clientId] = ws;
//            console.log(`${//clientId} connected`)//;
//            return;//
//        }

        

//        // Fo//rward message to target peer
//     //   let data = JSON.parse(msg);
////        let targetId = data.to;
//        if (clients[targetId] && clients[targetId//].readyState === WebSocket.OPEN) {
//    //        clien//ts[targetId].send(msg);
//   //     }

////        console.log(data);//

//    });

//    ws.on('close', () => {
// //       if (clientId) delete clients[clientId];
////        c//onsole.log(`${clientId} disconnected`);
//    });
//});

//console.log("Signaling server running on ws://0.0.0.0:8080");
