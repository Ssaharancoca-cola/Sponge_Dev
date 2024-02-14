var Common = {

        self: Common,
        selectedStartYear: '',
        selectedEndYear: '',

        MonthEnum: {
            JANUARY: 0,
            FEBRUARY: 1,
            MARCH: 2,
            APRIL: 3,
            MAY: 4,
            JUNE: 5,
            JULY: 6,
            AUGUST: 7,
            SEPTEMBER: 8,
            OCTOBER: 9,
            NOVEMBER: 10,
            DECEMBER: 11
        },
        MonthArray: ['Jan',
            'Feb',
            'Mar',
            'Apr',
            'May',
            'Jun',
            'Jul',
            'Aug',
            'Sep',
            'Oct',
            'Nov',
            'Dec'
        ],
        WeekEnum: {
            WEEK1: { START_DATE: 1, END_DATE: 7 },
            WEEK2: { START_DATE: 8, END_DATE: 14 },
            WEEK3: { START_DATE: 15, END_DATE: 21 },
            WEEK4: { START_DATE: 22, END_DATE: 28 }

        },

        HalfYearlyEnum: {
            H1: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JANUARY, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JUNE, 30) } },
            H2: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JULY, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.DECEMBER, 31) } }
        },
        QuarterlyEnum: {
            Q1: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JANUARY, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.MARCH, 31) } },
            Q2: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.APRIL, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JUNE, 30) } },
            Q3: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.JULY, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.SEPTEMBER, 30) } },
            Q4: function () { return { START_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.OCTOBER, 1), END_DATE: new Date(Common.selectedStartYear, Common.MonthEnum.DECEMBER, 31) } }
        },
        SearchMonth: function (myValue) {
            for (prop in Common.MonthEnum) {
                if (Common.MonthEnum[prop] == myValue) {
                    return prop;
                }
            }
        },
        DaysInMonth: function (month, year) {
            return new Date(year, month, 0).getDate();
        },
        MonthFromEnum: function (monthEnumValue) {
            return monthEnumValue + 1;
        },
        formatDate: function (value) {
            return Common.base10(value.getMonth()+1 ) + "/" + Common.base10(value.getDate()) + "/" + value.getFullYear();
        },
        formatDateToDateObject: function (fDate) {
            var dateObj = fDate.split('/');
            return new Date(dateObj[2], dateObj[0], dateObj[1]);
        },
        displayFormatDate: function (value) {
            return Common.base10(value.getDate()) + "/" + Common.MonthArray[value.getMonth()] + "/" + value.getFullYear();
        },

        base10: function (target) {
            return target < 10 ? '0' + target : target;
        },

        FillDropDownOptions: function (select, text, value) {
            $(select).append($('<option>', {
                value: value,
                text: text
            }));
        },
        DateFormatText: 'dd-MM-yy'


    };
    Date.prototype.addDays = function (days) {
        this.setDate(this.getDate() + parseInt(days));
        return this;
    };
