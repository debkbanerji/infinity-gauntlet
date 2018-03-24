// Get dependencies
const express = require('express');
const path = require('path');
const http = require('http');
const bodyParser = require('body-parser');
const cookieParser = require('cookie-parser');
const firebase = require('firebase');
let fs = require('fs');

firebaseCredentials = JSON.parse(fs.readFileSync('firebase-config.json'));
const firebaseApp = firebase.initializeApp(firebaseCredentials);
const database = firebaseApp.database();
console.log("Initialized firebase app");

const app = express();

/**
 * Get port from environment and store in Express.
 */
const port = process.env.PORT || '3000';
app.set('port', port);
console.log("Node server running on port " + port);

// Point static path to 'static' folder
app.use(express.static(path.join(__dirname, 'static'))).use(cookieParser());
console.log("Serving static from 'static' folder");

app.use(function (error, req, res, next) {
    console.log(req.originalUrl, ':', error.stack);
    res.render('500', {
        status: 500,
        url: req.url,
        title: 'Something broke :(',
        user: req.user,
        stateMessage: '',
        pageState: ''
    });
});

console.log("Using error logging");

// Parsers for POST data
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended: false}));

console.log("Using body parser");

console.log("Setting up database listeners");

const handId = 'hand_1';

let handStatus = [false, false, false, false];
let currFinger = 'idle';

const currHandRef = firebase.database().ref('hands/' + handId);
currHandRef.on('value', function (snapshot) {
    const val = snapshot.val();
    handStatus = val['hand_status'];
    currFinger = val['curr_finger'];
});

console.log("Setting up routes");

app.get('/curr-finger', (req, res) => {
    res.send(currFinger);
});

app.get('/hand-status', (req, res) => {
    res.send(handStatus);
});

app.get('/hand', (req, res) => {
    res.send({hand_status: handStatus, curr_finger: currFinger});
});

// Catch all other routes and return the current finger
app.get('*', (req, res) => {
    res.send(currFinger);
});

console.log("Catching all other routes and returning the index file");

/**
 * Create HTTP server.
 */
const server = http.createServer(app);

console.log("Created Server");
/**
 * Listen on provided port, on all network interfaces.
 */
server.listen(port, () => console.log(`Server running on port: ${port}`));