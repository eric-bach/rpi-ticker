const https = require('https');
const fs = require('fs');
const dotenv = require('dotenv');
dotenv.config();

function main() {
  var symbols = ['AMZN', 'BMO', 'DDOG', 'LYFT', 'MSFT', 'NIO', 'SNOW', 'TSLA'];

  // Clear file
  fs.writeFileSync('test.txt', ``);

  symbols.map((s) => {
    getQuote(s).then((r) => {
      console.log('Done: ', r);

      try {
        fs.writeFileSync('test.txt', `${s},${r.price},${r.change}\n`, {
          flag: 'a+',
        });
      } catch (err) {
        console.error(err);
      }
    });
  });
}

async function getQuote(symbol) {
  // Finnhub has 60 calls/min limit
  var url = `https://finnhub.io/api/v1/quote?symbol=${symbol}&token=${process.env.FINNHUB_API_KEY}`;

  var data = await get(url);

  return {
    symbol: symbol,
    price: data['c'],
    change: data['d'],
  };
}

function get(url) {
  return new Promise((resolve, reject) => {
    const req = https.get(url, (res) => {
      res.setEncoding('utf8');
      let body = '';

      res.on('data', (chunk) => {
        body += chunk;
      });

      res.on('end', () => {
        resolve(JSON.parse(body));
      });
    });

    req.on('error', (err) => {
      reject(err);
    });

    req.end();
  });
}

if (require.main === module) {
  main();
}
