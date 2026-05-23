const fs = require('fs');
fetch('https://provinces.open-api.vn/api/?depth=2')
    .then(res => res.text())
    .then(text => {
        fs.writeFileSync('wwwroot/data/provinces.json', text, 'utf8');
        console.log("Done");
    })
    .catch(console.error);
