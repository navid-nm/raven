function minMax(value, min, max) { return Math.min(Math.max(value, min), max);; }
function steps(steps) { return Math.ceil((minMax(0, 0.000001, 1)) * steps) * (1 / steps); }
function getSlope(aT, aA1, aA2) { return 3.0 * aT * aT + 2.0 * aT + aA1; }
function addNums(a, b) { return a + b; }

let results = [
    { Function: "steps", Result: steps(10) }, 
    { Function: "getSlope", Result: getSlope(1, 2, 3) }, 
    { Function: "addNums", Result: addNums(5, 5) }
]

console.table(results)
