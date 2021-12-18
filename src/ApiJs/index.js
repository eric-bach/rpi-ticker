const https = require('https');
const fs = require('fs');
const yahooFinance = require('yahoo-finance2').default; // NOTE the .default
const dotenv = require('dotenv');
dotenv.config();

function main() {
  var outputFolder =
    process.argv.slice(2).length > 0 ? process.argv.slice(2)[0] : '../../data';
  var quotesFile = `${outputFolder}/quotes.txt`;
  var newsFile = `${outputFolder}/headlines.txt`;

  // TODO Parameterize this
  var symbols = [
    'AMZN',
    'BMO.TO',
    'DDOG',
    'LYFT',
    'MSFT',
    'NIO',
    'SNOW',
    'TSLA',
  ];

  // Clear file
  // TODO Create folder/file if does not exist
  fs.writeFileSync(quotesFile, ``);
  fs.writeFileSync(newsFile, ``);

  symbols.map((s) => {
    getQuote(s).then((r) => {
      console.log(r);

      try {
        fs.writeFileSync(quotesFile, `${s},${r.price},${r.change}\n`, {
          flag: 'a+',
        });
      } catch (err) {
        console.error(err);
      }
    });
  });

  getNews().then((headlines) => {
    headlines.map((h) => {
      console.log(h);

      try {
        fs.writeFileSync(newsFile, `${h}\n`, {
          flag: 'a+',
        });
      } catch (err) {
        console.error(err);
      }
    });
  });
}

async function getQuote(symbol) {
  // Yahoo Finance
  const data = await yahooFinance.quoteSummary(symbol, { modules: ['price'] });

  return {
    symbol: data.price['symbol'],
    price: data.price['regularMarketPrice'],
    change: data.price['regularMarketChange'],
  };
}

async function getNews() {
  // Finnhub has 200 daily calls limit
  var url = `https://newsdata.io/api/1/news?apikey=${process.env.NEWSDATA_API_KEY}&language=en&country=ca&q=headlines`;
  var data = await get(url);

  return data.results.map((r) => r.title);
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
