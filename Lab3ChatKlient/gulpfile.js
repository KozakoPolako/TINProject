 const gulp = require('gulp');
 const sass = require("gulp-sass");
 const sourcemaps = require("gulp-sourcemaps");
 const autoprefixer = require("gulp-autoprefixer");
 const browserSync = require("browser-sync").create();
 const webpack = require("webpack");

 sass.compiler = require("sass");

 const server = (cb) => {
     browserSync.init({
         server: {
             baseDir: "./dist"
         },
         notify: false,
         host: "localhost",
         port: 3000,
         open: false,
         ghostMode: false
     });
     cb();
 }

 const css = () => {
     return gulp.src("src/scss/main.scss")
        .pipe(sourcemaps.init())
        .pipe(
            sass({
                outputStyle : "compressed"
            }).on("error",sass.logError)
        )
        .pipe(autoprefixer())
        .pipe(sourcemaps.write("."))
        .pipe(gulp.dest("dist/css"))
        .pipe(browserSync.stream());
 }
 
 const js = (cb) => {
     return webpack(require("./webpack.config.js"), (err,stats) => {
        if(err)
            throw err;
        console.log(stats);
        browserSync.reload();
        cb();
     });
 }

 const watch = (cb) => {
     gulp.watch("src/scss/**/*.scss",{usePolling:true}, gulp.series((css)));
     gulp.watch("src/js/**/*.js", gulp.series(js));
     gulp.watch("dist/**/*.html").on("change", browserSync.reload);
     cb();
 }

 exports.default = gulp.series(css,js,server,watch);
 exports.css = css;
 exports.watch = watch;
 exports.js = js;
