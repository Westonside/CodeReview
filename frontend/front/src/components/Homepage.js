import React, {useEffect} from "react";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import AddIcon from "@mui/icons-material/Add";
import {
    Alert,
    Autocomplete, Button, CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Fab,
    Grid,
    TextField
} from "@mui/material";
import PostsPane from "./PostsPane";
import MessagesPane from "./MessagesPane";

export function HomePage() {
    const [users, setUsers] = React.useState(null);
    const [user, setUser] = React.useState(window.localStorage.getItem("user"));
    const [open, setOpen] = React.useState(false);

    const [createUserUsername, setCreateUserUsername] = React.useState(null);
    const [createUserEmail, setCreateUserEmail] = React.useState(null);
    const [validation, setValidation] = React.useState(null);
    const [userCreated, setUserCreated] = React.useState("none");

    const createUser = () => {
        if(!createUserUsername) {
            setValidation("Username is required!");
            return;
        }
        if(!createUserEmail) {
            setValidation("Email is required!");
            return;
        }
        setValidation(null);

        const data = new FormData();

        data.set("Email", createUserEmail);
        data.set("Username", createUserUsername);
        data.set("Password", "superSecretPassword");

        setUserCreated("loading");

        fetch("/api/user/", {
            method: "POST",
            body: data
        })
            .then(response => {
                if(response.status === 200) {
                    setUserCreated("done");
                    setUser(createUserUsername);
                    window.localStorage.setItem("user", createUserUsername);
                    reloadUsers();
                } else {
                    response.json().then(data => {
                        if(data.hasErr) {
                            setValidation(data.data.errors[0]);
                            setUserCreated("none");
                        }
                    })
                }
            });
    }

    const userChange = (e) => {
        const newUser = e.target.innerText;
        setUser(newUser);
        window.localStorage.setItem("user", newUser);
    }

    const createUserUsernameChange = (e) => {
        setCreateUserUsername(e.target.value);
    }

    const createUserEmailChange = (e) => {
        setCreateUserEmail(e.target.value);
    }

    const reloadUsers = () => {
        fetch("/api/user/", {
            method: "GET"
        })
            .then(response => response.json())
            .then(data => {
                console.log(data);
                setUsers(data.data.list);
            });
    }

    if (!users) {
        reloadUsers();
        return (<h1>Loading...</h1>);
    }

    const openDialog = () => {
        setUserCreated("none");
        setOpen(true);
    }

    const closeDialog = () => {
        setOpen(false);
        setCreateUserEmail(null);
        setCreateUserUsername(null);
        setUserCreated("none");
    }

    return (<>
        <AppBar position={"static"}>
            <Toolbar>
                <Autocomplete renderInput={(params) => <TextField {...params} label="User"/>}
                              options={users.map(user => user.username)}
                              onChange={userChange}
                              fullWidth
                              defaultValue={user}
                />
                <Fab color="secondary" aria-label="add">
                    <AddIcon onClick={openDialog}/>
                </Fab>
            </Toolbar>
        </AppBar>
        <Dialog open={open} onClose={closeDialog}>
            <DialogTitle>Create User</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Please enter your details below to create an account
                </DialogContentText>
                {validation && <Alert severity={"error"}>{validation}</Alert>}
                <TextField
                    autoFocus
                    margin="dense"
                    label="Username"
                    fullWidth
                    onChange={createUserUsernameChange}
                />
                <TextField
                    autoFocus
                    margin="dense"
                    label="Email"
                    fullWidth
                    type="email"
                    onChange={createUserEmailChange}
                />
            </DialogContent>
                {
                    userCreated === "none" ?
                        <DialogActions>
                            <Button onClick={closeDialog}>Cancel</Button>
                            <Button onClick={createUser}>Create User</Button>
                        </DialogActions> :
                    userCreated === "loading" ?
                        <DialogActions>
                            <CircularProgress />
                        </DialogActions> :
                        <>
                            <Alert severity="success">User created!</Alert>
                            <DialogActions>
                                <Button onClick={closeDialog}>Done</Button>
                            </DialogActions>
                        </>
                }
        </Dialog>
        {
            user === null
            ?
                <Alert severity={"error"}>Please select or create a user to begin</Alert>
            :
                <Grid container spacing={4}>
                    <Grid container item xs={6} direction={"column"}>
                        <PostsPane user={user}/>
                    </Grid>
                    <Grid container item xs={6} direction={"column"}>
                        <MessagesPane user={user} users={users}/>
                    </Grid>
                </Grid>
        }
    </>);
}