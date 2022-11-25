import React from "react";
import {
    Alert, Autocomplete,
    Button, Card, CardContent, CircularProgress,
    Dialog, DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    TextareaAutosize, TextField,
    Typography
} from "@mui/material";

let key = 0;

export default function MessagesPane(props) {
    const user = props.user;
    const users = props.users;
    const [messages, setMessages] = React.useState(null);
    const [dialogOpen, setDialogOpen] = React.useState(false);
    const [validation, setValidation] = React.useState(null);
    const [messageSent, setMessageSent] = React.useState("none");
    const [messageContent, setMessageContent] = React.useState(null);
    const [convoUser, setConvoUser] = React.useState(null);
    const [messageDialogOpen, setMessageDialogOpen] = React.useState(false);
    const [messageDialogMessages, setMessageDialogMessages] = React.useState(null);

    const reload = () => {
        fetch(`/api/messages/${user}`, {
            method: "GET"
        })
            .then(response => response.json())
            .then(data => {
                setMessages(data.data);
                console.log(data);
            });
    }

    if (!messages) {
        reload();
        return (<h1>Loading...</h1>);
    }

    const createMessage = () => {
        setDialogOpen(true);
    };

    const dialogClose = () => {
        setDialogOpen(false);
        setMessageContent(null);
    };

    const messageContentChange = (e) => {
        setMessageContent(e.target.value);
        setMessageSent("none");
    }

    const sendMessage = () => {
        if (!messageContent) {
            setValidation("Message cannot be empty!");
            return;
        }
        fetch("/api/messages/create/", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                from: user,
                to: convoUser,
                message: messageContent
            })
        })
            .then(response => {
                if (response.status === 200) {
                    setMessageSent("done");
                    reload();
                    dialogClose();
                    openMessageDialog(convoUser);
                }
            })
        setMessageSent("loading");
    };

    const userChange = (e) => {
        setConvoUser(e.target.innerText);
    };

    const openMessageDialog = (them) => {
        fetch(`/api/messages/${user}/${them}`, {
            method: "GET"
        })
            .then(response => response.json())
            .then(data => {
                setMessageDialogMessages(data.data);
                setMessageDialogOpen(true);
                setConvoUser(them);
                console.log(data);
            });
    };

    const closeMessageDialog = () => {
        setMessageDialogOpen(false);
        setConvoUser(null);
        setMessageDialogMessages(null);
    }

    let messageComponents = messages.map(x =>
        <Message
            from={x.from}
            to={x.to}
            message={x.message}
            time={x.time}
            user={user}
            dialogFunc={openMessageDialog}
            key={key++}
        />);

    let messageRows = messageDialogMessages ? messageDialogMessages.map(x =>
        <MessageRow from={x.from} to={x.to} message={x.message} time={x.time} user={user} key={key++}/>
    ).reverse() : null;

    return <>
        <Typography variant="h1">
            Messages
        </Typography>
        <Button
            fullWidth
            onClick={createMessage}
            variant="contained"
            size="large"
        >
            <Typography variant="h5">
                New Conversation
            </Typography>
        </Button>
        {messageComponents}
        {messageComponents.length ? <></> : <Typography variant="h3">Start your first conversation now!</Typography>}
        <Dialog open={dialogOpen} onClose={dialogClose}>
            <DialogTitle>Start Conversation</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Write your message below
                </DialogContentText>
                {validation && <Alert severity={"error"}>{validation}</Alert>}
                <Autocomplete renderInput={(params) => <TextField {...params} label="User"/>}
                              options={users.map(user => user.username).filter(username => username != user)}
                              onChange={userChange}
                              fullWidth
                />
                <TextareaAutosize
                    placeholder="Message content"
                    style={{width: 500, height: 200}}
                    onChange={messageContentChange}
                >
                </TextareaAutosize>
            </DialogContent>
            {
                messageSent === "none" ?
                    <DialogActions>
                        <Button onClick={dialogClose}>Cancel</Button>
                        <Button onClick={sendMessage}>Send</Button>
                    </DialogActions> :
                    messageSent === "loading" ?
                        <DialogActions>
                            <CircularProgress/>
                        </DialogActions> :
                        <>
                            <Alert severity="success">Message sent!</Alert>
                            <DialogActions>
                                <Button onClick={dialogClose}>Done</Button>
                            </DialogActions>
                        </>
            }
        </Dialog>
        <Dialog
            open={messageDialogOpen}
            onClose={closeMessageDialog}
            PaperProps={{
                sx: {
                    maxHeight: "80vh"
                }
            }}
        >
            <DialogTitle>Messages with {convoUser}</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Write your message below
                </DialogContentText>
                {validation && <Alert severity={"error"}>{validation}</Alert>}
                {messageRows}
                <TextareaAutosize
                    placeholder="Message content"
                    style={{width: 500, height: 200}}
                    onChange={messageContentChange}
                >
                </TextareaAutosize>
            </DialogContent>
            {
                messageSent === "none" ?
                    <DialogActions>
                        <Button onClick={closeMessageDialog}>Cancel</Button>
                        <Button onClick={sendMessage}>Send</Button>
                    </DialogActions> :
                    messageSent === "loading" ?
                        <DialogActions>
                            <CircularProgress/>
                        </DialogActions> :
                        <>
                            <Alert severity="success">Message sent!</Alert>
                            <DialogActions>
                                <Button onClick={closeMessageDialog}>Done</Button>
                            </DialogActions>
                        </>
            }
        </Dialog>
    </>;
}

function Message(props) {
    let them;
    if (props.user === props.from) {
        them = props.to;
    } else {
        them = props.from;
    }

    const openDialog = () => {
        props.dialogFunc(them);
    };

    return (
        <Card onClick={openDialog} style={{cursor: "pointer"}}>
            <CardContent>
                <Typography color="text.secondary" variant="h4">
                    {them}
                </Typography>
                <Typography display="inline" variant="h3">
                    {props.user === props.from ? "You: " : them + ": "}
                </Typography>
                <Typography display="inline" variant="h2">
                    {props.message}
                </Typography>
            </CardContent>
        </Card>
    );
}

function MessageRow(props) {
    let them;
    if (props.user === props.from) {
        them = props.to;
    } else {
        them = props.from;
    }
    return (<Card>
        <CardContent>
            <Typography align={props.from === them ? "left" : "right"}>
                {props.message}
            </Typography>
        </CardContent>
    </Card>);
}