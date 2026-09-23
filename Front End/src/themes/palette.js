/**
 * Color intention that you want to used in your theme
 * @param {JsonObject} theme Theme customization object
 */

export default function themePalette(theme) {
    let result = {
        mode: theme?.customization?.navType,
        common: {
            black: theme.colors?.darkPaper
        },
        primary: {
            light: theme.colors?.primaryLight,
            main: theme.colors?.primaryMain,
            dark: theme.colors?.primaryDark,
            200: theme.colors?.primary200,
            800: theme.colors?.primary800,
            text: {
                colourful: 'white',
                light: theme.colors?.primaryDark
            },
            background: {
                colourful: theme.colors?.primaryDark,
                light: 'white'
            }
        },
        secondary: {
            light: theme.colors?.secondaryLight,
            main: theme.colors?.secondaryMain,
            dark: theme.colors?.secondaryDark,
            200: theme.colors?.secondary200,
            800: theme.colors?.secondary800,
            text: {
                colourful: 'white',
                light: theme.colors?.secondaryDark
            },
            background: {
                colourful: theme.colors?.secondaryDark,
                light: 'white'
            }
        },
        third: {
            light: theme.colors?.thirdLight,
            main: theme.colors?.thirdMain,
            dark: theme.colors?.thirdDark,
            200: theme.colors?.third200,
            800: theme.colors?.third800,
            text: {
                colourful: 'white',
                light: theme.colors?.grey700
            },
            background: {
                colourful: theme.colors?.thirdDark,
                light: 'white'
            }
        },
        fourth: {
            light: theme.colors?.successLight,
            main: theme.colors?.successMain,
            dark: theme.colors?.successDark,
            200: theme.colors?.success200,
            800: theme.colors?.success800,
            text: {
                colourful: 'white',
                light: theme.colors?.grey900
            },
            background: {
                colourful: theme.colors?.successDark,
                light: 'white'
            }
        },
        error: {
            light: theme.colors?.errorLight,
            main: theme.colors?.errorMain,
            dark: theme.colors?.errorDark,
            text: {
                colourful: 'white',
                light: theme.colors?.errorDark
            },
            background: {
                colourful: theme.colors?.errorDark,
                light: 'white'
            }
        },
        orange: {
            light: theme.colors?.orangeLight,
            main: theme.colors?.orangeMain,
            dark: theme.colors?.orangeDark
        },
        warning: {
            light: theme.colors?.warningLight,
            main: theme.colors?.warningMain,
            dark: theme.colors?.warningDark
        },
        success: {
            light: theme.colors?.successLight,
            200: theme.colors?.success200,
            main: theme.colors?.successMain,
            dark: theme.colors?.successDark
        },
        grey: {
            50: theme.colors?.grey50,
            100: theme.colors?.grey100,
            500: theme.darkTextSecondary,
            600: theme.heading,
            700: theme.darkTextPrimary,
            900: theme.textDark
        },
        dark: {
            light: theme.colors?.darkTextPrimary,
            main: theme.colors?.darkLevel1,
            dark: theme.colors?.darkLevel2,
            800: theme.colors?.darkBackground,
            900: theme.colors?.darkPaper
        },
        text: {
            primary: theme.darkTextPrimary,
            secondary: theme.darkTextSecondary,
            dark: theme.textDark,
            hint: theme.colors?.grey100
        },
        background: {
            paper: theme.paper,
            default: theme.backgroundDefault
        },
        lightTheme: {
            primary: {
                main: theme.colors?.primaryMain,
                10: theme.colors?.primaryDark,
                25: theme.colors?.primary800,
                75: theme.colors?.primary200,
                100: theme.colors?.primaryLight,
                text: theme.colors?.grey900,
                background: 'white'
            },
            secondary: {
                main: theme.colors?.secondaryMain,
                10: theme.colors?.secondaryDark,
                25: theme.colors?.secondary800,
                75: theme.colors?.secondary200,
                100: theme.colors?.secondaryLight,
                text: theme.colors?.grey900,
                background: 'white'
            },
            third: {
                main: theme.colors?.thirdMain,
                10: theme.colors?.thirdDark,
                25: theme.colors?.third800,
                75: theme.colors?.third200,
                100: theme.colors?.thirdLight,
                text: theme.colors?.grey900,
                background: 'white'
            },
            fourth: {
                main: theme.colors?.successMain,
                10: theme.colors?.successDark,
                25: theme.colors?.success800,
                75: theme.colors?.success200,
                100: theme.colors?.successLight,
                text: theme.colors?.grey900,
                background: 'white'
            }
        },
        colourfulTheme: {
            primary: {
                main: theme.colors?.primaryMain,
                10: theme.colors?.primaryLight,
                25: theme.colors?.primary200,
                75: theme.colors?.primary800,
                100: theme.colors?.primaryDark,
                text: 'white',
                background: theme.colors?.primaryDark
            },
            secondary: {
                main: theme.colors?.secondaryMain,
                10: theme.colors?.secondaryLight,
                25: theme.colors?.secondary200,
                75: theme.colors?.secondary800,
                100: theme.colors?.secondaryDark,
                text: 'white',
                background: theme.colors?.secondaryDark
            },
            third: {
                main: theme.colors?.thirdMain,
                10: theme.colors?.thirdLight,
                25: theme.colors?.third200,
                75: theme.colors?.third800,
                100: theme.colors?.thirdDark,
                text: 'white',
                background: theme.colors?.thirdDark
            },
            fourth: {
                main: theme.colors?.successMain,
                10: theme.colors?.successLight,
                25: theme.colors?.success200,
                75: theme.colors?.success800,
                100: theme.colors?.successDark,
                text: 'white',
                background: theme.colors?.successDark
            }
        }
    };

    return result;
}
