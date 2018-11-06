

const compose = (...fnc) => arg => fnc.reduce((composed, f) => f(composed), arg);
 



const elem = (str) => document.getElementById(str);

const getCurrentColor = (elem) => {

	var bgColor = localStorage.getItem("color-" + elem.id);

	if (!bgColor) { return bgColor }
	if (bgColor.includes("rgb(")) {
		var current = bgColor.split("rgb(")[1].split(",");
		return [current[0], current[1], current[2].slice(0, current[2].length - 1)];
	} else if (bgColor.includes("rgba(")) {
		var current = bgColor.split("rgba(")[1].split(",");
		return [current[0], current[1], current[2]];
	} else {
		return [
			bgColor.slice(0, 2),
			bgColor.slice(2, 4),
			bgColor.slice(4, 6)]
	};
}

const getColors = (htmlElement) => ({
	fromColor: htmlElement.getAttribute("from-color"),
	toColor: htmlElement.getAttribute("to-color"),
	currentColor: getCurrentColor(htmlElement)
});

const checkInput = (colors) => ({
	...colors,
	fromColor: colors.fromColor.toString().length === 6 &&
		(parseInt(colors.fromColor, 16) ||
			parseInt(colors.fromColor, 16) === 0) ? colors.fromColor : NaN,
	toColor: colors.toColor.toString().length === 6 &&
		(parseInt(colors.toColor, 16) ||
			parseInt(colors.toColor, 16) === 0) ? colors.toColor : NaN
})

const parseColors = (colors) => {

	if ((colors.fromColor) && (colors.toColor)) {
		return {
			current: {
				red: colors.currentColor ? parseFloat(colors.currentColor[0]) : parseInt(colors.fromColor.slice(0, 2), 16),
				green: colors.currentColor ? parseFloat(colors.currentColor[1]) : parseInt(colors.fromColor.slice(2, 4), 16),
				blue: colors.currentColor ? parseFloat(colors.currentColor[2]) : parseInt(colors.fromColor.slice(4, 6), 16)
			},
			from: {
				red: parseInt(colors.fromColor.slice(0, 2), 16),
				green: parseInt(colors.fromColor.slice(2, 4), 16),
				blue: parseInt(colors.fromColor.slice(4, 6), 16)
			},
			to: {
				red: parseInt(colors.toColor.slice(0, 2), 16),
				green: parseInt(colors.toColor.slice(2, 4), 16),
				blue: parseInt(colors.toColor.slice(4, 6), 16)
			}
		};
	} else { throw new Error("The tag property 'from-color' or 'to-color' is invalid. Please enter string from '000000' to 'ffffff'.", "ThirdApp.js", 68); }
}



const count = (str) => (colors) => {
	var reds = Math.abs(colors.from.red - colors.to.red) ? Math.abs(colors.from.red - colors.to.red) : 1;
	var greens = Math.abs(colors.from.green - colors.to.green) ? Math.abs(colors.from.green - colors.to.green) : 1;
	var blues = Math.abs(colors.from.blue - colors.to.blue) ? Math.abs(colors.from.blue - colors.to.blue) : 1;
	var min = Math.min(reds, greens, blues);

	return {
		...colors,
		coefficients: {
			rcoef: parseInt(localStorage.getItem("red-coef-" + str)) || (reds / min),
			gcoef: parseInt(localStorage.getItem("green-coef-" + str)) || (greens / min),
			bcoef: parseInt(localStorage.getItem("blue-coef-" + str)) || (blues / min)
		},
		signs: {
			rsing: (colors.current.red <= colors.from.red && colors.current.red <= colors.to.red) ? 1 : (colors.current.red >= colors.to.red && colors.current.red >= colors.from.red) ? -1 : NaN,
			gsing: (colors.current.green <= colors.from.green && colors.current.green <= colors.to.green) ? 1 : (colors.current.green >= colors.to.green && colors.current.green >= colors.from.green) ? -1 : NaN,
			bsing: (colors.current.blue <= colors.from.blue && colors.current.blue <= colors.to.blue) ? 1 : (colors.current.blue >= colors.to.blue && colors.current.blue >= colors.from.blue) ? -1 : NaN
		}
	}

}

const coefs = (colors) => ({
	...colors,
	coefficients: {
		rcoef: (colors.signs.rsing === 1 ? Math.abs(colors.coefficients.rcoef) : colors.signs.rsing === -1 && colors.coefficients.rcoef > 0 ? -1 * colors.coefficients.rcoef : colors.coefficients.rcoef),
		gcoef: (colors.signs.gsing === 1 ? Math.abs(colors.coefficients.gcoef) : colors.signs.gsing === -1 && colors.coefficients.gcoef > 0 ? -1 * colors.coefficients.gcoef : colors.coefficients.gcoef),
		bcoef: (colors.signs.bsing === 1 ? Math.abs(colors.coefficients.bcoef) : colors.signs.bsing === -1 && colors.coefficients.bcoef > 0 ? -1 * colors.coefficients.bcoef : colors.coefficients.bcoef)
	}
})

const maxAbs = (...arguments) => arguments.reduce((x, y) => Math.max(Math.abs(x), Math.abs(y)));
const generateNewColor = (str) => (colors) => {

	colors.current.red += colors.coefficients.rcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	colors.current.green += colors.coefficients.gcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	colors.current.blue += colors.coefficients.bcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);

	localStorage.setItem("red-coef-" + str, colors.coefficients.rcoef);
	localStorage.setItem("green-coef-" + str, colors.coefficients.gcoef);
	localStorage.setItem("blue-coef-" + str, colors.coefficients.bcoef);
	localStorage.setItem("color-" + str, "rgb(" + colors.current.red + ", " + colors.current.green + ", " + colors.current.blue + ")");
	return { red: Math.round(colors.current.red), green: Math.round(colors.current.green), blue: Math.round(colors.current.blue) };
}
const generateNewColorSlowly = (str) => (colors) => {
	if (!colors.signs.rsing) {
		colors.current.red += colors.coefficients.rcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	} else if (!colors.signs.gsing) {
		colors.current.green += colors.coefficients.gcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	} else if (!colors.signs.bsing) {
		colors.current.blue += colors.coefficients.bcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	}
	if (colors.signs.rsing && colors.signs.gsing && colors.signs.bsing) {
		colors.current.red += colors.coefficients.rcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
		colors.current.green += colors.coefficients.gcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
		colors.current.blue += colors.coefficients.bcoef / maxAbs(colors.coefficients.rcoef, colors.coefficients.gcoef, colors.coefficients.bcoef);
	}

	localStorage.setItem("red-coef-" + str, colors.coefficients.rcoef);
	localStorage.setItem("green-coef-" + str, colors.coefficients.gcoef);
	localStorage.setItem("blue-coef-" + str, colors.coefficients.bcoef);
	localStorage.setItem("color-" + str, "rgb(" + colors.current.red + ", " + colors.current.green + ", " + colors.current.blue + ")");
	return { red: Math.round(colors.current.red), green: Math.round(colors.current.green), blue: Math.round(colors.current.blue) };
}

const render = (str) => (color) => {
	var element = document.getElementById(str);
	var style = element.getAttribute("colorize");
	element.style[style.toString()] = " rgb(" + color.red + ", " + color.green + ", " + color.blue + ")";

}

const debug = (x) => {
	const copy = JSON.parse(JSON.stringify(x));
	console.log(copy); return x;
};

const changeColor = (args) => compose(
	elem, getColors, checkInput, parseColors, count(args), coefs, generateNewColor(args), render(args)
)(args);

const startColorize = (arg) => setInterval(changeColor, 100, [arg]);
