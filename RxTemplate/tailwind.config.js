/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["Components/**/*.razor"],
  plugins: [require("@tailwindcss/typography"), require("daisyui")],
  // You may removed the daisyui themes you don't need to reduce the minified wwwroot/css/app.css file size
  daisyui: {
    themes: [
      //"light",
      //"dark",
      "cupcake",
      "bumblebee",
      "emerald",
      "corporate",
      "synthwave",
      "retro",
      "cyberpunk",
      "valentine",
      "halloween",
      "garden",
      "forest",
      "aqua",
      "lofi",
      "pastel",
      "fantasy",
      "wireframe",
      "black",
      "luxury",
      "dracula",
      "cmyk",
      "autumn",
      "business",
      "acid",
      "lemonade",
      "night",
      "coffee",
      "winter",
      "dim",
      "nord",
      "sunset",
      {
        // theme modification example - replace the dark or light theme and apply customizations
        light: {
          ...require("daisyui/src/theming/themes")["nord"],
          primary: "#405874",
          "--rounded-box": "0.4rem",
          "--rounded-btn": "0.2rem",
          "--rounded-badge": "0.4rem",
          "--tab-radius": "0.2rem",
        },
      },
      {
        dark: {
          ...require("daisyui/src/theming/themes")["dark"],
          primary: "#d1fae5",
          "--rounded-box": "0.4rem",
          "--rounded-btn": "0.2rem",
          "--rounded-badge": "0.4rem",
          "--tab-radius": "0.2rem",
        },
      },
    ], // false: only light + dark | true: all themes | array: specific themes like this ["light", "dark", "cupcake"]
    base: true, // applies background color and foreground color for root element by default
    styled: true, // include daisyUI colors and design decisions for all components
    utils: true, // adds responsive and modifier utility classes
    prefix: "", // prefix for daisyUI classnames (components, modifiers and responsive class names. Not colors)
    logs: true, // Shows info about daisyUI version and used config in the console when building your CSS
    themeRoot: ":root", // The element that receives theme color CSS variables
  },
  // [data-theme="<your selected dark theme name"]
  darkMode: ["selector", '[data-theme="dark"]'],
};
