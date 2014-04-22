(function() {
	"use strict";
	
	var HomeViewModel = function() {
		var self = this;
		self.db = window.Db;
		self.isBusy = false;
		self.IsHasData = false;

		self.seriesList = ko.observableArray();
		self.topNew50List = ko.observableArray();
		self.searchResultList = ko.observableArray();
		self.historySeries = ko.observableArray();
		self.selectedSeries = ko.observable("");
		self.currentChapterIndex = 0;
		self.searchText = ko.observable("");
		self.searchTextResult = ko.observable("");
		self.selectedChapter = ko.observable("");
		self.pageList = ko.observableArray([]);

		self.db.exists('historySeries', function(obj) {
			if (!obj) {
				self.db.save({
					key : 'historySeries',
					Series : []
				});
			}
		});
			
		self.db.get('historySeries', function(historySeries) {
			historySeries.Series.forEach(function(obj) {
				self.historySeries.push(ko.observable(obj));
			});
		});

		self.ExitApp = function() {
			navigator.app.exitApp();
		};
		self.OpenPage = function(Page) {
			$('.Page.ActivePage').removeClass('ActivePage');
			$('#' + Page).addClass('ActivePage');
		};
		self.OpenTab = function(data, current) {
			var indent = null;
			if (typeof data == "string") {
				indent = data;
			} else {
				indent = data.id;
			}
			if (indent == "ListTab") {
				if (!self.isBusy) {
					if (!self.IsHasData) {
						//self.isBusy = true;
						//self.GetNumberOfSeries();
					}
				}
			}
			if (indent == "ListNewTab") {
				//self.GetTopNew50();
			}
			if (current) {
				$("#" + current.id + "Content").css("display", "none");
			} else {
				$("#" + indent + "Content").siblings('.TabPage').css("display",
						"none");
			}
			$("#" + indent + "Content").css("display", "block");
		};
		self.InitContent = function() {
			self.GetTopNew50(self.GetNumberOfSeries);
		};
		self.RefreshSeriesList = function() {
			self.GetSeriesPart(self.seriesPart);
		};
		self.RefreshTopNewList = function() {
			self.GetTopNew50();
		};
		self.OpenSeriesInfo = function(data, event, selectedItem) {
			$("#BusyIndicator").show();
			$.ajax({
				type : 'GET',
				url : window.apiUrl + "api/manga/GetSeriesInfo?id="
						+ selectedItem.Id,
				success : function(data) {
					data.MangaChapters = {};
					self.selectedSeries(data);
					$.ajax({
						type : 'GET',
						url : window.apiUrl + "api/manga/GetChapter?series="
								+ selectedItem.Id,
						success : function(data) {
							var item = self.selectedSeries();
							item.MangaChapters = data;
							self.selectedSeries(item);
						},
						error : function(e) {
							self.ShowApiRequestError();
						},
						complete : function(data) {
							$("#BusyIndicator").hide();
						}
					});
					self.OpenPage('SeriesView');
					self.OpenTab('MangaSeriesInfoTab');
				},
				error : function(e) {
					self.ShowApiRequestError();
					$("#BusyIndicator").hide();
				},
				complete : function(data) {
				}
			});
		};
		self.Previous10series = function(data, event) {
			if (!self.isBusy) {
				if (self.seriesPart - 1 > 0) {
					self.isBusy = true;
					self.GetSeriesPart(--self.seriesPart);
				}
			}
		};
		self.Next10series = function(data, event) {
			if (!self.isBusy) {
				if (self.seriesPart + 1 <= self.numberOfSeriesPart) {
					self.isBusy = true;
					self.GetSeriesPart(++self.seriesPart);
				}
			}
		};
		self.GetNumberOfSeries = function() {
			$("#BusyIndicator").show();
			$.ajax({
						type : "GET",
						url : window.apiUrl + "api/manga/GetNumberOfSeries",
						success : function(data) {
							self.numberOfSeries = parseInt(data);
							self.numberOfSeriesPart = (self.numberOfSeries % 10 === 0 ? self.numberOfSeries / 10
									: self.numberOfSeries / 10 + 1);
							self.seriesPart = 1;
							self.IsHasData = true;
							self.GetSeriesPart(self.seriesPart);
						},
						error : function(e) {
							self.ShowApiRequestError();
						},
						complete : function(data) {
							self.isBusy = false;
							$("#BusyIndicator").hide();
						}
					});
		}
		
		self.GetTopNew50 = function(callback) {
			$("#BusyIndicator").show();
			$.ajax({
				type : "GET",
				url : window.apiUrl + "api/manga/GetTopNew50",
				success : function(data) {
					self.topNew50List.removeAll();
					for (var i = 0; i < data.length; i++) {
						self.topNew50List.push(data[i]);
					}
					$(".TabPageContent").scrollTop();
					if (typeof (callback) == "function" && callback != null)
						callback();
				},
				error : function(e) {
					self.ShowApiRequestError();
				},
				complete : function(data) {
					self.isBusy = false;
					$("#BusyIndicator").hide();
				}
			});
		};
		self.ShowApiRequestError = function() {
			alert("Network's not available. Please check internet connection!");
		};
		self.GetSeriesPart = function(seed) {
			$("#BusyIndicator").show();
			$.ajax({
				type : "GET",
				url : window.apiUrl + "api/manga/getpart?part=" + seed,
				dataType : 'json',
				success : function(data) {
					if (data.length > 0) {
						self.seriesList.removeAll();
						for (var i = 0; i < data.length; i++) {
							self.seriesList.push(data[i]);
						}
						$(".TabPageContent").scrollTop();
					}
				},
				error : function(e) {
					self.ShowApiRequestError();
				},
				complete : function(data) {
					self.isBusy = false;
					$("#BusyIndicator").hide();
				}
			});
		};
		self.FindSeries = function(data, event) {
			var path = null;
			if (self.searchText() === null || self.searchText() === "") {
				path = "";
			} else {
				path = self.searchText();
				while (path.indexOf(" ") > 0) {
					path = path.replace(" ", "_");
				}
			}
			path = window.apiUrl + "api/manga/search?keywords="
					+ path.replace(" ", "_");
			$("#BusyIndicator").show();
			$.ajax({
				type : "GET",
				url : path,
				timeout : 10000,
				dataType : 'json',
				success : function(data) {
					if (data) {
						self.searchResultList.removeAll();
						for (var i = 0; i < data.length; i++) {
							self.searchResultList.push(data[i]);
						}
						if (data.length == 0) {
							self.searchTextResult("Not found.");
						} else
							self.searchTextResult(data.length + " results.");
					}
				},
				error : function(e) {
					self.ShowApiRequestError();
				},
				complete : function(data) {
					$("#BusyIndicator").hide();
				}
			});
		};
		self.GetAndFillpageList = function() {
			$("#BusyIndicator").show();
			self.pageList([]);
			$.ajax({
				type : "GET",
				url : window.apiUrl + "api/manga/getchapter?series="
						+ self.selectedSeries().Id + "&chapter="
						+ self.selectedChapter().Id,
				success : function(data) {
					if (data.length > 0) {
						for (var i = 0; i < data.length; i++) {
							self.pageList.push({
								Url : data[i]
							});
						}
						$('.pageList img.lazy').lazy({
							bind : "event",
							combined : true
						});
					}
				},
				error : function(e) {
					self.ShowApiRequestError();
				},
				complete : function(data) {
					self.isBusy = false;
					$("#BusyIndicator").hide();
				}
			});
		};
		self.SelectChapter = function(selectedItem, event, index) {
			self.ToggleViewList();
			self.pageList.removeAll();
			self.selectedChapter(selectedItem);
			self.currentChapterIndex = index();
			// self.OpenTab('ChapterViewTab');
			self.GetAndFillpageList();
			var existence = ko.utils.arrayFirst(self.historySeries(), function(
					obj) {
				return obj().Id == self.selectedSeries().Id;
			});
			if (existence != null) {
				self.historySeries.remove(function(item) {
					return item().Id == existence().Id;
				});
				var obj = existence();
				obj.currentChapter = self.selectedChapter();
				existence(obj);
				self.historySeries.unshift(existence);
			} else {
				var obj = self.selectedSeries();
				obj.currentChapter = self.selectedChapter();
				self.historySeries.unshift(ko.observable(obj));
			}
			if (self.historySeries().length == 10)
				self.historySeries.pop();
			self.db.save({
				key : 'historySeries',
				Series : ko.toJS(self.historySeries())
			});
		};
		self.CloseFullScreen = function() {
			self.pageList.removeAll();
			self.ToggleViewList();
		};
		self.BackToHome = function() {
			self.OpenPage("HomeView");
		};
		self.IsDisplayFull = false;
		self.NextChapter = function() {
			if (self.currentChapterIndex - 1 >= 0) {
				self
						.selectedChapter(self.selectedSeries().MangaChapters[self.currentChapterIndex - 1]);
				self.currentChapterIndex--;
				self.GetAndFillpageList();
			} else {
				alert("You are reading latest chapter!!! \nUpcomming chapter will be available soon.");
			}
		};
		self.PreviousChapter = function() {
			if (self.currentChapterIndex + 1 < self.selectedSeries().MangaChapters.length) {
				self
						.selectedChapter(self.selectedSeries().MangaChapters[self.currentChapterIndex + 1]);
				self.currentChapterIndex++;
				self.GetAndFillpageList();
			}
		};
		self.ToggleViewList = function() {
			if (self.IsDisplayFull) {
				$("#ChapterViewTabContent").hide();
				$("#MangaSeriesInfoTabContent").show();
				$(".pageList").css("zoom", "100%");
				self.IsDisplayFull = false;
			} else {
				$("#MangaSeriesInfoTabContent").hide();
				$("#ChapterViewTabContent").show();
				self.IsDisplayFull = true;
			}
		}
		self.SwipeLeft = function() {
			$('#TabItems').data('cmenu').navigate("next");
		};
		self.SwipeRight = function() {
			$('#TabItems').data('cmenu').navigate("prev");
		};
		self.PinchIn = function() {
			$(".pageList").css("zoom", "-=0.1%");
		};
		self.PinchOut = function() {
			$(".pageList").css("zoom", "+=0.1%");
		};
		$(document).ready(function() {
			self.OpenTab('ListNewTab');
			$('#TabItems').cmenu(null, self.OpenTab);
		});
		return self;
	};
	var SeriesViewModel = function() {
	};

	var app = {
		Initialize : function() {
			window.comeFirst();
		}
	};

	window.apiUrl = "http://www.mangaportal.vn/";
	//window.apiUrl =  "http://192.168.1.67/";
	window.Db = new Lawnchair({
		adapter : 'dom',
		name : 'UserSettingDb'
	}, function(store) {
	});
	window.comeFirst = function() {
		var self = this;
		if (typeof self.isComeFirst == "undefined") {
			self.isComeFirst = true;
		} else {
			navigator.splashscreen.hide();
		}
	};
	window.ViewModels = {
		HomeViewModel : new HomeViewModel(),
		SeriesViewModel : new SeriesViewModel()
	};
	$(document).on('deviceready', app.Initialize, false);
	$(document).ready(function() {
		window.comeFirst();
		jQuery.support.cors = true;
		ko.applyBindings(window.ViewModels);
		$("#BusyIndicator div").sprite({
			fps : 12,
			no_of_frames : 8
		});
		window.ViewModels.HomeViewModel.InitContent();
		document.addEventListener("backbutton", yourCallbackFunction, false);

	});
})();