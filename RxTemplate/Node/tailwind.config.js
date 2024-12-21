/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "../Components/**/*.{razor,razor.css}",
    "src/js/**/*.{js,jsx,ts,tsx}",
  ],
  // The skinned components require these styles. tailwindcss is not able
  // to discover the usage of classes that are set as [Parameter] values, so
  // they must be safe listed.
  safelist: [
    "alert",
    "alert-error",
    "alert-success",
    "btn",
    "btn-ghost",
    "btn-neutral",
    "btn-primary",
    "checkbox",
    "checkbox-error",
    "cursor-pointer",
    "duration-1000",
    "ease-out",
    "file-input",
    "file-input-bordered",
    "file-input-error",
    "flex",
    "flex-col",
    "font-bold",
    "font-semibold",
    "form-control",
    "gap-y-2",
    "h-full",
    "input",
    "input-bordered",
    "input-error",
    "justify-center",
    "label",
    "label-text",
    "label-text-alt",
    "loading",
    "loading-bars",
    "max-sm:flex-col",
    "modal",
    "modal-action",
    "modal-bottom",
    "modal-box",
    "opacity-0",
    "progress",
    "progress-success",
    "radio",
    "radio-error",
    "range",
    "range-error",
    "select",
    "select-bordered",
    "select-error",
    "sm:modal-middle",
    "text-accent",
    "text-error",
    "text-lg",
    "text-primary",
    "textarea",
    "textarea-bordered",
    "textarea-error",
    "toast",
    "toast-center",
    "toast-top",
    "toggle",
    "toggle-error",
    "tooltip",
    "tooltip-info",
    "tooltip-left",
    "transition-opacity",
    "w-full",
    "z-[100]",
  ],
  theme: {
    fontFamily: {
      sans: [
        "-apple-system",
        "BlinkMacSystemFont",
        "Segoe UI",
        "Roboto",
        "Arial",
        "Helvetica",
        "sans-serif",
        "Apple Color Emoji",
        "Segoe UI Emoji",
        "Segoe UI Symbol",
      ],
      mono: ["Courier New", "Courier", "monospace"],
    },
  },
  plugins: [require("@tailwindcss/typography"), require("daisyui")],
  // You may removed the themes you don't need to reduce the minified wwwroot/css/app.css file size
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
        },
      },
      {
        dark: {
          ...require("daisyui/src/theming/themes")["dark"],
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
