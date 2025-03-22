/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["Components/**/*.razor"],
  plugins: [require("@tailwindcss/typography")],
  // [data-theme="<your selected dark theme name"]
  darkMode: ["selector", '[data-theme="dark"]'],
};
