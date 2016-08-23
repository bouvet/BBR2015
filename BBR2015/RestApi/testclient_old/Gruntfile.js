module.exports = function (grunt) {
require('load-grunt-tasks')(grunt);

// Define the configuration for all the tasks
grunt.initConfig({
    // Project settings
    config: {
        // Configurable paths
        app: ''
    },
    // Watches files for changes and runs tasks based on the changed files
    watch: {
        livereload: {
            options: {
                livereload: 35730
            },
            files: [
                '**/**.js',
                '**/**.css',
                '**/**.html',
                '**/**.svg'
            ]
        }
    },
    // The actual grunt server settings
    connect: {
        options: {
            port: 9001,
            livereload: 35730,
            hostname: '0.0.0.0'
        },
        livereload: {
            options: {
                open: true,
                base: [
                    '.tmp',
                    '<%= config.app %>'
                ]
            }
        },
    },
});
grunt.registerTask('serve', function (target) {
    grunt.task.run([
        'connect:livereload',
        'watch'
        ]);
    });
};