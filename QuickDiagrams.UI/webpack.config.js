const path = require('path');

const CleanPlugin = require('clean-webpack-plugin');
const ExtractCssPlugin = require('mini-css-extract-plugin');
const VueLoaderPlugin = require('vue-loader/lib/plugin');

const sourceDir = path.resolve(__dirname, './src');
//const styleDir = path.resolve(sourceDir, './styles');

const buildDir = path.resolve('../QuickDiagrams.Api/wwwroot/dist');

const getSourcePath = srcPath => path.resolve(sourceDir, srcPath);

const babelLoader = {
    loader: 'babel-loader',
    options: {
        presets: [['@babel/preset-env', { modules: false, useBuiltIns: 'usage', corejs: 3 }]],
        plugins: [
            '@babel/plugin-proposal-object-rest-spread',
            ['@babel/plugin-proposal-decorators', { legacy: true }],
            '@babel/plugin-proposal-class-properties'
        ]
    }
};

module.exports = {
    devtool: 'source-map',
    entry: {
        master: getSourcePath('index.js'),
        //style: `${styleDir}/style.scss`
    },
    mode: 'development',
    module: {
        rules: [

            {
                test: /\.scss$/,
                use: [ExtractCssPlugin.loader, 'css-loader', 'sass-loader']
            },

            {
                exclude: /(node_modules|bower_components)/,
                include: sourceDir,
                test: /\.js?/,
                use: [{ ...babelLoader }]
            },

            {
                test: /\.css$/,
                use: [ExtractCssPlugin.loader, 'css-loader']
            },

            {
                test: /\.vue$/,
                loader: 'vue-loader'
            }
        ]
    },
    optimization: {
        splitChunks: {
            cacheGroups: {
                vendors: {
                    chunks: 'all',
                    name: 'vendor',
                    test: /[\\/]node_modules[\\/]/
                }
            }
        },
    },
    output: {
        filename: '[name].min.js',
        chunkFilename: '[name].min.js',
        globalObject: 'this',
        path: `${buildDir}`,
        publicPath: '/wwwroot/dist/'
    },
    plugins: [
        // Deletes build directory before rebuilds.
        new CleanPlugin({
            cleanOnceBeforeBuildPatterns: [`${buildDir}/**`],
            dangerouslyAllowCleanPatternsOutsideProject: true
        }),

        // Extract CSS from bundles, and compile them into a single CSS file
        new ExtractCssPlugin({
            filename: '[name].min.css'
        }),

        // Clone any other rules you have defined and apply them to the corresponding language blocks in .vue files.
        new VueLoaderPlugin()
    ],
};