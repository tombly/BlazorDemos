const path = require('path');

module.exports = {
  entry: path.resolve(__dirname, 'telerik-blazor-custom.js'),
  module: {
    rules: [
      {
        test: /\.(js)$/,
        exclude: /node_modules/
      }
    ]
  },
  resolve: {
    extensions: ['*', '.js']
  },
  output: {
    path: path.resolve(__dirname, '.'),
    filename: 'telerik-blazor-custom.js',
    library: 'TelerikBlazor',
    libraryTarget: "var",
  },
  devServer: {
    static: path.resolve(__dirname, '.'),
  },
};
