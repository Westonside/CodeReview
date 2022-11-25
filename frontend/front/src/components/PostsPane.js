import React from "react";
import {
    Alert,
    Button,
    Card,
    CardContent, CircularProgress,
    Dialog, DialogActions,
    DialogContent, DialogContentText,
    DialogTitle,
    TextareaAutosize, Typography
} from "@mui/material";

let key = 0;

export default function PostsPane(props) {
    const user = props.user;
    const [posts, setPosts] = React.useState(null);
    const [dialogOpen, setDialogOpen] = React.useState(false);
    const [validation, setValidation] = React.useState(null);
    const [postCreated, setPostCreated] = React.useState("none");
    const [postContent, setPostContent] = React.useState(null);

    const reload = () => {
        fetch("/api/posts/all", {
            method: "GET"
        })
            .then(response => response.json())
            .then(data => {
                setPosts(data);
            });
    }

    if (!posts) {
        reload();
        return (<h1>Loading...</h1>);
    }

    const createPost = () => {
        setDialogOpen(true);
    };

    const dialogClose = () => {
        setDialogOpen(false);
        setPostContent(null);
        setPostCreated("none");
    };

    const postContentChange = (e) => {
        setPostContent(e.target.value);
    }

    const publishPost = () => {
        if (!postContent) {
            setValidation("Post cannot be empty!");
            return;
        }
        fetch("/api/posts/create", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                uid: user,
                content: postContent
            })
        })
            .then(response => {
                if(response.status === 201) {
                    setPostCreated("done");
                    reload();
                }
            })
        setPostCreated("loading");
    };

    let postComponents = posts.map(x => <Post user={x.uid} content={x.content} key={key++}/>).reverse();

    return <>
        <Typography variant="h1">
            Posts
        </Typography>
        <Button
            fullWidth
            onClick={createPost}
            variant="contained"
            size="large"
        >
            <Typography variant="h5">
                Create Post
            </Typography>
        </Button>
        {postComponents}
        {postComponents.length ? <></> : <Typography variant="h3">It's very quiet... be the first to say something!</Typography>}
        <Dialog open={dialogOpen} onClose={dialogClose}>
            <DialogTitle>Create Post</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Write your post below
                </DialogContentText>
                {validation && <Alert severity={"error"}>{validation}</Alert>}
                <TextareaAutosize
                    placeholder="Post content"
                    style={{width: 500, height: 200}}
                    onChange={postContentChange}
                >
                </TextareaAutosize>
            </DialogContent>
            {
                postCreated === "none" ?
                    <DialogActions>
                        <Button onClick={dialogClose}>Cancel</Button>
                        <Button onClick={publishPost}>Post</Button>
                    </DialogActions> :
                    postCreated === "loading" ?
                        <DialogActions>
                            <CircularProgress/>
                        </DialogActions> :
                        <>
                            <Alert severity="success">Post created!</Alert>
                            <DialogActions>
                                <Button onClick={dialogClose}>Done</Button>
                            </DialogActions>
                        </>
            }
        </Dialog>
    </>;
}

function Post(props) {
    return (
        <Card>
            <CardContent>
                <Typography color="text.secondary" variant="h4">
                    {props.user}
                </Typography>
                <Typography variant="h2">
                    {props.content}
                </Typography>
            </CardContent>
        </Card>
    );
}