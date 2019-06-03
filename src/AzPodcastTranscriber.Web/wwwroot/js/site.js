// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var $container = $("#transcription-container"),
    $segments = $container.find(".transcription-segment"),
    $audio = $("audio")[0],
    offset = -1,
    typingTimer,
    doneTypingtimer,
    interval = 1000;

//on keyup, start the countdown
$('textarea').keyup(function () {
    clearTimeout(doneTypingtimer);
    doneTypingtimer = setTimeout(function () { $audio.play(); }, interval);
    
});

$('textarea').on( "input", function () {
    $audio.pause();
    
});


$("#resume-button").click(function (event) {
    playTranscript(false);
    $audio.play();
});

$("#pause-button").click(function (event) {
    $audio.pause();
});

function playTranscript(pause) { // This is possibly the only code that I wrote where had to seriously think about the logic.
    if (offset < $audio.currentTime) {

        $segments.each(function () {

            if ($(this).data("offset") > $audio.currentTime) {
                if (offset !== -1 && pause) { // First Iteration
                    $audio.pause();
                    return false;
                }
                highlightSegment($(this));
                return false; // break
            }
        });
    }
}

function highlightSegment(element) {
    offset = element.data("offset"); // Set global variable. Here Offset > audio.currentTime

    var textToHighlight = element.prev(".transcription-segment");
    textToHighlight.addClass("highlighted");

    textToHighlight.prevAll(".transcription-segment").removeClass("highlighted").addClass("dimmed");
    textToHighlight.nextAll(".transcription-segment").removeClass("highlighted dimmed");
    $container.scrollTo(textToHighlight, 1000);
}