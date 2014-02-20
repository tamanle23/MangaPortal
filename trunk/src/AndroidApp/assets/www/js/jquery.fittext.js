(function( $ ){

  $.fn.fitText = function( kompressor, options ) {

    var compressor = kompressor || 1,
        settings = $.extend({
          'minFontSize' : Number.NEGATIVE_INFINITY,
          'maxFontSize' : Number.POSITIVE_INFINITY 
        }, options);

    return this.each(function(){
      var scope = $(this);
   
      var resizer = function ()
	  {
    	var fSize=Math.max(Math.min(scope.width() / (compressor*10), parseFloat(settings.maxFontSize)), parseFloat(settings.minFontSize));
        scope.css('font-size',fSize );
        scope.css('margin-top',(scope.height()-fSize)/2);
        
	  };
      resizer();
      $(window).on('resize.fittext orientationchange.fittext', resizer);
    });
  };
})( jQuery );
