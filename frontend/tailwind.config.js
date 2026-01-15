/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#FF8C00',
          light: '#FFD700',
        },
      },
    },
  },
  plugins: [],
};
