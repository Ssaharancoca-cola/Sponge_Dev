var FillGranularDropdown = function () {
    //debugger;
    var select = $('#GranularTime');
    if (select.length) {
        var granularTimeLevel = $('#hdnOnTimeLevel').val();
        var frequency = $('#hdnFrequency').val();
        $("#GranularTime option[value!='']").remove();
        if (frequency == 'YEARLY') {
            FillYearly_GranularDropdown(select, granularTimeLevel);
        }
        if (frequency == 'HALF_YEARLY') {
            FillHalfYearly_GranularDropdown(select, granularTimeLevel);
        }
        if (frequency == 'QUARTERLY') {
            FillQuarterly_GranularDropdown(select, granularTimeLevel);
        }
        if (frequency == 'MONTHLY') {
            FillMonthly_GranularDropdown(select, granularTimeLevel);
        }
        if (frequency == 'WEEKLY') {
            FillWeekly_GranularDropdown(select, granularTimeLevel);
        }
    }


}



var FillYearly_GranularDropdown = function (select, granularTimeValue) {
    var startYear, endYear, pattern;
    var pattern = $('#hdnOnTimeText').val();
    var obj = pattern.split('-');
    var startYear = obj[0].toString(), endYear = obj[1].toString();
    if (granularTimeValue == 'HALF_YEARLY') {
        Common.FillDropDownOptions(select, 'H1', ProcessGranularCodeFor_HalfYearOrQuarter(startYear, Common.MonthEnum.JANUARY, Common.MonthEnum.JUNE));
        Common.FillDropDownOptions(select, 'H2', ProcessGranularCodeFor_HalfYearOrQuarter(endYear, Common.MonthEnum.JULY, Common.MonthEnum.DECEMBER));
    }
    if (granularTimeValue == 'QUARTERLY') {
        Common.FillDropDownOptions(select, 'Q1', ProcessGranularCodeFor_HalfYearOrQuarter(startYear, Common.MonthEnum.JANUARY, Common.MonthEnum.MARCH));
        Common.FillDropDownOptions(select, 'Q2', ProcessGranularCodeFor_HalfYearOrQuarter(startYear, Common.MonthEnum.APRIL, Common.MonthEnum.JUNE));
        Common.FillDropDownOptions(select, 'Q3', ProcessGranularCodeFor_HalfYearOrQuarter(endYear, Common.MonthEnum.JULY, Common.MonthEnum.SEPTEMBER));
        Common.FillDropDownOptions(select, 'Q4', ProcessGranularCodeFor_HalfYearOrQuarter(endYear, Common.MonthEnum.OCTOBER, Common.MonthEnum.DECEMBER));
    }
    if (granularTimeValue == 'MONTHLY') {
        for (var i = Common.MonthFromEnum(Common.MonthEnum.JANUARY); i <= Common.MonthFromEnum(Common.MonthEnum.JUNE); i++) {
            var monthName = Common.SearchMonth(i - 1);
            Common.FillDropDownOptions(select, monthName, startYear + Common.base10(i));
        }
        for (var i = Common.MonthFromEnum(Common.MonthEnum.JULY); i <= Common.MonthFromEnum(Common.MonthEnum.DECEMBER); i++) {
            var monthName = Common.SearchMonth(i - 1);
            Common.FillDropDownOptions(select, monthName, endYear + Common.base10(i));
        }
    }
}
var FillHalfYearly_GranularDropdown = function (select, granularTimeValue) {
    var startYear, endYear, pattern;
    var pattern = $('#hdnOnTimeText').val();
    var arrayVal = pattern.split('-');
    startYear = arrayVal[0];
    arrayVal = arrayVal[1].split(',');
    endYear = arrayVal[0];
    if (arrayVal[1] == 'H1') {
        if (granularTimeValue == 'QUARTERLY') {
            Common.FillDropDownOptions(select, 'Q1', ProcessGranularCodeFor_HalfYearOrQuarter(startYear, Common.MonthEnum.JANUARY, Common.MonthEnum.MARCH));
            Common.FillDropDownOptions(select, 'Q2', ProcessGranularCodeFor_HalfYearOrQuarter(startYear, Common.MonthEnum.APRIL, Common.MonthEnum.JUNE));
        }
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.JANUARY); i <= Common.MonthFromEnum(Common.MonthEnum.JUNE); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, startYear + Common.base10(i));
            }
        }
    }
    if (arrayVal[1] == 'H2') {
        if (granularTimeValue == 'QUARTERLY') {
            Common.FillDropDownOptions(select, 'Q3', ProcessGranularCodeFor_HalfYearOrQuarter(endYear, Common.MonthEnum.JULY, Common.MonthEnum.SEPTEMBER));
            Common.FillDropDownOptions(select, 'Q4', ProcessGranularCodeFor_HalfYearOrQuarter(endYear, Common.MonthEnum.OCTOBER, Common.MonthEnum.DECEMBER));
        }
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.JULY); i <= Common.MonthFromEnum(Common.MonthEnum.DECEMBER); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, endYear + Common.base10(i));
            }
        }
    }

}

var FillQuarterly_GranularDropdown = function (select, granularTimeValue) {
    var startYear, endYear, pattern;
    var pattern = $('#hdnOnTimeText').val();
    var arrayVal = pattern.split('-');
    startYear = arrayVal[0];
    arrayVal = arrayVal[1].split(',');
    endYear = arrayVal[0];
    if (arrayVal[1] == 'Q1') {
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.JANUARY); i <= Common.MonthFromEnum(Common.MonthEnum.MARCH); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, startYear + Common.base10(i));
            }
        }
    }
    if (arrayVal[1] == 'Q2') {
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.APRIL); i <= Common.MonthFromEnum(Common.MonthEnum.JUNE); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, startYear + Common.base10(i));
            }
        }
    }
    if (arrayVal[1] == 'Q3') {
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.JULY); i <= Common.MonthFromEnum(Common.MonthEnum.SEPTEMBER); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, endYear + Common.base10(i));
            }
        }
    }
    if (arrayVal[1] == 'Q4') {
        if (granularTimeValue == 'MONTHLY') {
            for (var i = Common.MonthFromEnum(Common.MonthEnum.OCTOBER); i <= Common.MonthFromEnum(Common.MonthEnum.DECEMBER); i++) {
                var monthName = Common.SearchMonth(i - 1);
                Common.FillDropDownOptions(select, monthName, endYear + Common.base10(i));
            }
        }
    }
}
var FillMonthly_GranularDropdown = function (select, granularTimeValue) {
    var year, pattern;
    pattern = $('#hdnOnTimeText').val();
    var arrayVal = pattern.split('-');
    var month = arrayVal[0].toUpperCase();
    year = arrayVal[1];
    if (granularTimeValue == 'MONTHLY') {

        var dt = new Date();
        var monthName = Common.SearchMonth(dt.getMonth());
        //Common.FillDropDownOptions(select, monthName, year + Common.base10(dt.getMonth() + 1));
        var currentTime = new Date();
        var current_year = currentTime.getFullYear();
        var display_month_name = monthName + "-" + current_year;
        Common.FillDropDownOptions(select, display_month_name, year + Common.base10(dt.getMonth() + 1));
    }

    if (granularTimeValue == 'WEEKLY') {
        for (i = 1; i <= 4; i++) {
            Common.FillDropDownOptions(select, 'W' + i, year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + i);
        }
    }
    if (granularTimeValue == 'DAILY') {
        for (i = 1; i <= Common.DaysInMonth(Common.MonthFromEnum(Common.MonthEnum[month]), year); i++) {
            Common.FillDropDownOptions(select, Common.formatDate(new Date(year, Common.MonthEnum[month], i)), year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + Common.base10(i));
        }
    }
}

var FillWeekly_GranularDropdown = function (select, granularTimeValue) {
    var year, pattern;
    pattern = $('#hdnOnTimeText').val();
    var arrayVal = pattern.split('-');
    var month = arrayVal[0].toUpperCase();
    arrayVal = arrayVal[1].split(',');
    var year = arrayVal[0];

    if (granularTimeValue == 'DAILY') {
        if (arrayVal[1] == 'W1') {
            for (i = Common.WeekEnum.WEEK1.START_DATE; i <= Common.WeekEnum.WEEK1.END_DATE; i++) {
                Common.FillDropDownOptions(select, Common.formatDate(new Date(year, Common.MonthEnum[month], i)), year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + Common.base10(i));
            }
        }
        if (arrayVal[1] == 'W2') {
            for (i = Common.WeekEnum.WEEK2.START_DATE; i <= Common.WeekEnum.WEEK2.END_DATE; i++) {
                Common.FillDropDownOptions(select, Common.formatDate(new Date(year, Common.MonthEnum[month], i)), year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + Common.base10(i));
            }
        }
        if (arrayVal[1] == 'W3') {
            for (i = Common.WeekEnum.WEEK3.START_DATE; i <= Common.WeekEnum.WEEK3.END_DATE; i++) {
                Common.FillDropDownOptions(select, Common.formatDate(new Date(year, Common.MonthEnum[month], i)), year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + Common.base10(i));
            }
        }
        if (arrayVal[1] == 'W4') {
            for (i = Common.WeekEnum.WEEK4.START_DATE; i <= Common.DaysInMonth(Common.MonthFromEnum(Common.MonthEnum[month]), year); i++) {
                Common.FillDropDownOptions(select, Common.formatDate(new Date(year, Common.MonthEnum[month], i)), year + Common.base10(Common.MonthFromEnum(Common.MonthEnum[month])) + Common.base10(i));
            }
        }
    }
}
var FillYearlyGranularStartEndDate = function (startDate, endDate, granularity) {
    //debugger;
    var periodicStartDate, periodicEndDate, result = {};

    if (granularity == 'H1') {
        periodicStartDate = startDate;
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 7, 0);
    }
    else if (granularity == 'H2') {
        periodicStartDate = startDate.setMonth(startDate.getMonth() + 6);
        periodicEndDate = endDate;
    }
    /*************************************************/
    else if (granularity == 'Q1') {
        periodicStartDate = startDate;
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 4, 0);
    }
    else if (granularity == 'Q2') {
        periodicStartDate = startDate.setMonth(startDate.getMonth() + 4);
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 7, 0);
    }
    else if (granularity == 'Q3') {
        periodicStartDate = startDate.setMonth(startDate.getMonth() + 7);
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 10, 0);
    }
    else if (granularity == 'Q4') {
        periodicStartDate = startDate.setMonth(startDate.getMonth() + 10);
        periodicEndDate = endDate
    }
    /*************************************************/
    else {
        return FillMonthlyStartEndDate(startDate, endDate, granularity);
    }
    result["periodicStartDate"] = periodicStartDate;
    result["periodicEndDate"] = periodicEndDate;
    return result;

}
var FillHalfYearlyGranularStartEndDate = function (startDate, endDate, granularity) {
    var periodicStartDate, periodicEndDate;
    if (granularity == 'Q1' || granularity == 'Q3') {
        periodicStartDate = startDate;
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 4, 0);
    }
    else if (granularity == 'Q2' || granularity == 'Q4') {
        periodicStartDate = startDate.setMonth(startDate.getMonth() + 4);
        periodicEndDate = new Date(periodicStartDate.getFullYear(), periodicStartDate.getMonth() + 7, 0);
    }
    else {
        return FillMonthlyStartEndDate(granularity);
    }
    return { periodicStartDate: periodicStartDate, periodicEndDate: periodicEndDate };
}

var FillQuarterlyGranularStartEndDate = function (startDate, endDate, granularity) {
    var periodicStartDate, periodicEndDate;

    return FillMonthlyStartEndDate(granularity);
}

var FillMonthlyGranularStartEndDate = function (startDate, endDate, granularity) {
    var periodicStartDate, periodicEndDate;
    if (granularity == 'W1') {
        periodicStartDate = startDate
        periodicEndDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK1.END_DATE);
    }
    else if (granularity == 'W2') {
        periodicStartDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK2.START_DATE);
        periodicEndDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK2.END_DATE);
    }
    else if (granularity == 'W3') {
        periodicStartDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK3.START_DATE);
        periodicEndDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK3.END_DATE);
    }
    else if (granularity == 'W4') {
        periodicStartDate = new Date(startDate.getFullYear(), startDate.getMonth(), Common.WeekEnum.WEEK4.START_DATE);
        periodicEndDate = endDate;
    }
    else {
        periodicStartDate = Common.formatDateToDateObject(startDate);
        periodicEndDate = Common.formatDateToDateObject(endDate);
    }


    return { periodicStartDate: periodicStartDate, periodicEndDate: periodicEndDate };
}
var FillWeeklyGranularStartEndDate = function (startDate, endDate, granularity) {
    var periodicStartDate, periodicEndDate;
    periodicStartDate = Common.formatDateToDateObject(startDate);
    periodicEndDate = Common.formatDateToDateObject(endDate);
    return { periodicStartDate: periodicStartDate, periodicEndDate: periodicEndDate };
}

var FillMonthlyStartEndDate = function (startDate, endDate, granularity) {
    var s = Common.MonthEnum[granularity.toUpperCase()] + 1;
    var year = "";
    if (s > 7) {

        year = startDate.getFullYear();
    }
    else {
        year = endDate.getFullYear();
    }

    return {
        periodicStartDate: new Date(year, Common.MonthEnum[granularity.toUpperCase()], 1),
        periodicEndDate: new Date(year, Common.MonthEnum[granularity.toUpperCase()] + 1, 0)
    }
}




var ProcessGranularCodeFor_HalfYearOrQuarter = function (year, startMonthEnumKey, endMonthEnumKey) {
    return year + Common.base10(Common.MonthFromEnum(startMonthEnumKey)) + year + Common.base10(Common.MonthFromEnum(endMonthEnumKey));
}

