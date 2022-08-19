const ws = require('ws');

const wss = new ws.WebSocketServer({ port: 8080 });

var perSecond = 0;

wss.on('connection', function connection(ws) {
  console.log('client connected!');

  var ping = 0;

  setInterval(() => {
    ping = Date.now();
    ws.send('ping');
  }, 1000);

  ws.on('message', function message(data) {
    data = data.toString()
    // console.log(data)
    if (data === 'pong') {
      console.log('Ping: ' + (Date.now() - ping) + 'ms');
    }
    // console.log('received: %s', data);
    perSecond++;
  });
});

setInterval(() => {
  console.log('requests per second: ' + perSecond);
  perSecond = 0;
}, 1000);
