import createStyles from "@material-ui/core/styles/createStyles";
import makeStyles from "@material-ui/core/styles/makeStyles";
import {Theme} from "@material-ui/core/styles/createTheme";
import {alpha} from "@material-ui/core";

export const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            flexGrow: 1
        },
        //#region App Bar
        appBar: {
            backgroundColor: '#fff',
            color: theme.palette.text.primary
        },
        plainAppBar: {
            boxShadow: undefined,
            backgroundColor: 'rgba(0,0,0,0.7)',
            color: theme.palette.primary.contrastText,
            ['& a']: {
                color: theme.palette.primary.contrastText,
            }
        },
        title: {
            flexGrow: 1,
            marginLeft: 15
        },
        toolbar: {
            maxWidth: 1170,
            width: '85%',
            margin: '0px auto',
            [theme.breakpoints.down('sm')]: {
                ...theme.mixins.toolbar,
                maxWidth: 'auto',
                width: '100%',
                margin: 0,
            },
        },
        navigation: {
            textAlign: 'right',
            verticalAlign: 'middle',
            listStyleType: 'none',
            display: 'inherit',
            ['& li']: {
                verticalAlign: 'middle',
                alignItems: 'center',
                display: 'flex',
                marginLeft: 20,
            },
            [theme.breakpoints.down('sm')]: {
                display: 'none'
            },
        },
        link: {
            display: 'inline',
            textTransform: 'uppercase',
            fontWeight: 'bolder',
            fontSize: '0.98em',
            textDecoration: 'none',
            color: theme.palette.text.primary,
            verticalAlign: 'middle',
            ['&:hover']: {
                color: theme.palette.primary.main
            }
        },
        //#endregion
        mainWrapper: {
            backgroundSize: 'cover',
            background: 'url(main-background.png) center right #000',
            [theme.breakpoints.down('sm')]: {
                background: 'url(main-background.png) bottom left #000',
            }
        },
        //#region Page
        main: {
            maxWidth: 1170,
            width: '85%',
            margin: '0px auto',
            paddingBottom: 100,
            color: theme.palette.primary.contrastText,
            [theme.breakpoints.down('sm')]: {
                paddingBottom: 60,
            },
        },
        welcome: {
            display: 'block',
            marginTop: 130,
            [theme.breakpoints.down('sm')]: {
                marginTop: 40,
            },
        },
        description: {
            display: 'block',
            maxWidth: '50%',
            marginTop: 10,
            [theme.breakpoints.down('sm')]: {
                marginTop: 40,
                maxWidth: '100%',
                textAlign: 'right'
            },
        },
        buttons: {
            marginTop: 30,
            [theme.breakpoints.down('sm')]: {
                textAlign: 'right',
            },
        },
        button: {
            marginTop: 20,
            marginRight: 35,
            [theme.breakpoints.down('sm')]: {
                marginRight: 0,
                marginLeft: 35,
            },
        },
        contentWrapper: {},
        content: {
            maxWidth: 1170,
            width: '85%',
            margin: '0px auto',
            paddingBottom: 100
        },
        contentTitle: {
            marginTop: 50
        },
        contributeContent: {
            marginTop: 20
        },
        //#endregion
        footerWrapper: {
            backgroundColor: '#f1f3f3',
            padding: '60px 0 60px 0'
        },
        footer: {
            textAlign: 'center',
            color: alpha(theme.palette.text.primary, 0.6),
            maxWidth: 600,
            width: '85%',
            margin: '0 auto',
            ['& div']: {
                marginTop: 8
            },
            ['& a']: {
                color: theme.palette.primary.main
            }
        },
    }),
);