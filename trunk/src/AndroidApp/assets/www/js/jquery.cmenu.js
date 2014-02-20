(function($, undefined)
{
	$.cmenu = function(options, element, callback)
	{
		this.$el = $(element);
		this.initialize(options);
		this.$callback = callback;
	};
	$.cmenu.defaults = {
		current : 0, // index of current item
		interval : 2000
	// time between transitions
	};
	$.cmenu.prototype = {
		initialize : function(options)
		{
			this.options = $.extend(true, {}, $.cmenu.defaults, options);
			// support for 3d / 2d transforms and transitions
			this.support3d = Modernizr.csstransforms3d;
			this.support2d = Modernizr.csstransforms;
			this.supportTrans = Modernizr.csstransitions;
			this.$wrapper = this.$el.find('.cmenu-wrapper');

			this.$items = this.$wrapper.children();
			this.itemsCount = this.$items.length;

			this.$nav = this.$el.find('nav');

			if (this.itemsCount < 3)
			{
				this.$nav.remove();
				return false;
			}
			this.current = this.options.current;
			this.isAnim = false;
			this.$items.css({
				'opacity' : 0,
				'visibility' : 'hidden'
			});
			this.validate();
			this.layout();
			// load the events
			this._loadEvents();



		},
		validate : function()
		{

			if (this.options.current < 0
					|| this.options.current > this.itemsCount - 1)
			{
				this.current = 0;
			}
		},
		layout : function()
		{

			// current, left and right items
			this.setItems();

			// current item is not changed
			// left and right one are rotated and translated
			var leftCSS, rightCSS, currentCSS;

			if (this.support3d && this.supportTrans)
			{

				leftCSS = {
					'-webkit-transform' : 'translateX(-25%) translateZ(-200px) rotateY(45deg)',
					'-moz-transform' : 'translateX(-25%) translateZ(-200px) rotateY(45deg)',
					'-o-transform' : 'translateX(-25%) translateZ(-200px) rotateY(45deg)',
					'-ms-transform' : 'translateX(-25%) translateZ(-200px) rotateY(45deg)',
					'transform' : 'translateX(-25%) translateZ(-200px) rotateY(45deg)'
				};

				rightCSS = {
					'-webkit-transform' : 'translateX(25%) translateZ(-200px) rotateY(-45deg)',
					'-moz-transform' : 'translateX(25%) translateZ(-200px) rotateY(-45deg)',
					'-o-transform' : 'translateX(25%) translateZ(-200px) rotateY(-45deg)',
					'-ms-transform' : 'translateX(25%) translateZ(-200px) rotateY(-45deg)',
					'transform' : 'translateX(25%) translateZ(-200px) rotateY(-45deg)'
				};

				leftCSS.opacity = 1;
				leftCSS.visibility = 'visible';
				rightCSS.opacity = 1;
				rightCSS.visibility = 'visible';

			}
			else if (this.support2d && this.supportTrans)
			{

				leftCSS = {
					'-webkit-transform' : 'translate(-25%) scale(0.8)',
					'-moz-transform' : 'translate(-25%) scale(0.8)',
					'-o-transform' : 'translate(-25%) scale(0.8)',
					'-ms-transform' : 'translate(-25%) scale(0.8)',
					'transform' : 'translate(-25%) scale(0.8)',
					'z-index' : 2
				};

				rightCSS = {
					'-webkit-transform' : 'translate(25%) scale(0.8)',
					'-moz-transform' : 'translate(25%) scale(0.8)',
					'-o-transform' : 'translate(25%) scale(0.8)',
					'-ms-transform' : 'translate(25%) scale(0.8)',
					'transform' : 'translate(25%) scale(0.8)',
					'z-index' : 2
				};

				currentCSS = {
					'z-index' : 3
				};

				leftCSS.opacity = 1;
				leftCSS.visibility = 'visible';
				rightCSS.opacity = 1;
				rightCSS.visibility = 'visible';

			}

			this.$leftItm.css(leftCSS || {});
			this.$rightItm.css(rightCSS || {});

			this.$currentItm.css(currentCSS || {}).css({
				'opacity' : 1,
				'visibility' : 'visible'
			}).addClass('ActiveTab');

		},
		setItems : function()
		{

			this.$items.removeClass('ActiveTab');

			this.$currentItm = this.$items.eq(this.current);
			this.$leftItm = (this.current === 0) ? this.$items
					.eq(this.itemsCount - 1) : this.$items.eq(this.current - 1);
			this.$rightItm = (this.current === this.itemsCount - 1) ? this.$items
					.eq(0)
					: this.$items.eq(this.current + 1);

			if (!this.support3d && this.support2d && this.supportTrans)
			{

				this.$items.css('z-index', 2);
				this.$currentItm.css('z-index', 3);

			}

			// next & previous items
			if (this.itemsCount > 3)
			{

				// next item
				this.$nextItm = (this.$rightItm.index() === this.itemsCount - 1) ? this.$items
						.eq(0)
						: this.$rightItm.next();
				this.$nextItm.css(this._getCoordinates('outright'));

				// previous item
				this.$prevItm = (this.$leftItm.index() === 0) ? this.$items
						.eq(this.itemsCount - 1) : this.$leftItm.prev();
				this.$prevItm.css(this._getCoordinates('outleft'));

			}

		},
		_loadEvents : function()
		{

			var self = this;



			this.$wrapper.on('webkitTransitionEnd.cmenu transitionend.cmenu OTransitionEnd.cmenu',
							function(event)
							{

								self.$currentItm.addClass('ActiveTab');
								self.$items.removeClass('cmenu-transition');
								self.isAnim = false;

							});

		},
		_getCoordinates : function(position)
		{

			if (this.support3d && this.supportTrans)
			{

				switch (position)
				{
				case 'outleft':
					return {
						'-webkit-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
						'-moz-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
						'-o-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
						'-ms-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
						'transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'outright':
					return {
						'-webkit-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
						'-moz-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
						'-o-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
						'-ms-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
						'transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'left':
					return {
						'-webkit-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
						'-moz-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
						'-o-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
						'-ms-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
						'transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				case 'right':
					return {
						'-webkit-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
						'-moz-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
						'-o-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
						'-ms-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
						'transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				case 'center':
					return {
						'-webkit-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
						'-moz-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
						'-o-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
						'-ms-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
						'transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				}
				;

			} else if (this.support2d && this.supportTrans)
			{

				switch (position)
				{
				case 'outleft':
					return {
						'-webkit-transform' : 'translate(-450px) scale(0.7)',
						'-moz-transform' : 'translate(-450px) scale(0.7)',
						'-o-transform' : 'translate(-450px) scale(0.7)',
						'-ms-transform' : 'translate(-450px) scale(0.7)',
						'transform' : 'translate(-450px) scale(0.7)',
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'outright':
					return {
						'-webkit-transform' : 'translate(450px) scale(0.7)',
						'-moz-transform' : 'translate(450px) scale(0.7)',
						'-o-transform' : 'translate(450px) scale(0.7)',
						'-ms-transform' : 'translate(450px) scale(0.7)',
						'transform' : 'translate(450px) scale(0.7)',
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'left':
					return {
						'-webkit-transform' : 'translate(-25%) scale(0.8)',
						'-moz-transform' : 'translate(-25%) scale(0.8)',
						'-o-transform' : 'translate(-25%) scale(0.8)',
						'-ms-transform' : 'translate(-25%) scale(0.8)',
						'transform' : 'translate(-25%) scale(0.8)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				case 'right':
					return {
						'-webkit-transform' : 'translate(25%) scale(0.8)',
						'-moz-transform' : 'translate(25%) scale(0.8)',
						'-o-transform' : 'translate(25%) scale(0.8)',
						'-ms-transform' : 'translate(25%) scale(0.8)',
						'transform' : 'translate(25%) scale(0.8)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				case 'center':
					return {
						'-webkit-transform' : 'translate(0px) scale(1)',
						'-moz-transform' : 'translate(0px) scale(1)',
						'-o-transform' : 'translate(0px) scale(1)',
						'-ms-transform' : 'translate(0px) scale(1)',
						'transform' : 'translate(0px) scale(1)',
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				}
				;

			} else
			{

				switch (position)
				{
				case 'outleft':
				case 'outright':
				case 'left':
				case 'right':
					return {
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'center':
					return {
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				}
				;

			}

		},
		navigate : function(dir)
		{

			if (this.supportTrans && this.isAnim)
				return false;

			this.isAnim = true;

			switch (dir)
			{

			case 'next':

				this.current = this.$rightItm.index();

				// current item moves left
				this.$currentItm.addClass('cmenu-transition').css(
						this._getCoordinates('left'));

				// right item moves to the center
				this.$rightItm.addClass('cmenu-transition').css(
						this._getCoordinates('center'));

				// next item moves to the right
				if (this.$nextItm)
				{

					// left item moves out
					this.$leftItm.addClass('cmenu-transition').css(
							this._getCoordinates('outleft'));

					this.$nextItm.addClass('cmenu-transition').css(
							this._getCoordinates('right'));

				} else
				{

					// left item moves right
					this.$leftItm.addClass('cmenu-transition').css(
							this._getCoordinates('right'));

				}
				break;

			case 'prev':

				this.current = this.$leftItm.index();

				// current item moves right
				this.$currentItm.addClass('cmenu-transition').css(
						this._getCoordinates('right'));

				// left item moves to the center
				this.$leftItm.addClass('cmenu-transition').css(
						this._getCoordinates('center'));

				// prev item moves to the left
				if (this.$prevItm)
				{

					// right item moves out
					this.$rightItm.addClass('cmenu-transition').css(
							this._getCoordinates('outright'));

					this.$prevItm.addClass('cmenu-transition').css(
							this._getCoordinates('left'));

				} else
				{

					// right item moves left
					this.$rightItm.addClass('cmenu-transition').css(
							this._getCoordinates('left'));

				}
				break;

			}
			;
			var current = this.$currentItm[0];

			this.setItems();

			if (!this.supportTrans)
				this.$currentItm.addClass('ActiveTab');
			this.$callback(this.$currentItm[0], current);

		},

		destroy : function()
		{
			this.$wrapper.off('.cmenu');

		}
	};

	var logError = function(message)
	{
		if (this.console)
		{
			console.error(message);
		}
	};
	$.fn.cmenu = function(options, activeCallback)
	{

		if (typeof options === 'string')
		{
			var args = Array.prototype.slice.call(arguments, 1);
			this
					.each(function()
					{
						var instance = $.data(this, 'cmenu');
						if (!instance)
						{
				
							return;
						}
						if (!$.isFunction(instance[options])
								|| options.charAt(0) === "_")
						{
							logError("no such method '" + options
									+ "' for cmenu instance");
							return;
						}
						instance[options].apply(instance, args);
					});
		} else
		{
			this.each(function()
			{

				var instance = $.data(this, 'cmenu');
				if (!instance)
				{
					$.data(this, 'cmenu', new $.cmenu(options, this,
							activeCallback));
				}
			});

		}
		return this;
	};
})(jQuery);