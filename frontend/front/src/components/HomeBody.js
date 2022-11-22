import { Grid } from "@mui/material";
import React, { useState } from "react";
import { Paper } from "@mui/material";
import { createTheme, ThemeProvider, styled } from '@mui/material/styles';
import { Box } from "@mui/system";
import { Stack } from "@mui/material";
import Typography from "@mui/material";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
// import WatchList from "./WatchList";
// import BuyList from "./BuyList";
// import TopStocks from "./TopStocks";
const HomeBody = () =>{
    const Item = styled(Paper)(({ theme }) => ({
        ...theme.typography.body2,
        textAlign: 'center',
        color: theme.palette.text.secondary,
        minHeight: 100,
        lineHeight: '60px',
        
      }));

    const startValues = [1,2,3]
    const [postContent, setPostContent] = useState("");
    const [postUser, setPostUser] = useState("");


    const [messageUserOne, setMessageUserOne] = useState("");
    const [messageUserTwo, setMessageUserTwo] = useState("");
    const [contentMessage, setContentMessage] = useState("");

    

      const sendMessage = () =>{

      }


      const sendPost = () =>{
        
      }


   return(
    <Box sx={{ flexGrow: 2, marginTop:5, paddingLeft:5,paddingRight:5 }}>
        <TextField id="outlined-basic" label="Username" variant="outlined" value={postUser}  onChange={(e)=>{setPostUser(e.target.value)}}/>
        <TextField id="outlined-basic" label="Post Content" variant="outlined"  value={postContent} onChange={(e) =>{setPostContent(e.target.value)}}/>
        <Button variant="text" onClick={sendPost}>Submit</Button>

        <Grid container spacing={2}>
            <Grid item xs={12}>
                <p>Top Stocks</p>
                {/* <TopStocks /> */}
            </Grid>
            <Grid item container spacing={4} sx={{marginTop:5}}>
                <Grid item xs={7}>
                    <Item>
                        
                        {startValues.map((val) =>{
                            return(
                                <Item>
                                    <p>{val}</p>
                                    <p>{`${val} user`}</p>
                                    </Item>
                            )
                        })}
                        {/* <WatchList /> */}
                    </Item>
                </Grid>
                <Grid item xs={4}>
                <TextField id="outlined-basic" label="User sending message" variant="outlined" value={messageUserOne} onChange={(e)=>{setMessageUserOne(e.target.value)}}/>
                <TextField id="outlined-basic" label="User to send message" variant="outlined" value={messageUserTwo} onChange={(e)=>{setMessageUserTwo(e.target.value)}}/>
                <TextField id="outlined-basic" label="Message content" variant="outlined" value={contentMessage} onChange={(e)=>{setContentMessage(e.target.value)}} style={{marginTop: 5 }}/>
                <Button variant="text" onClick={sendMessage}>Submit</Button>
                </Grid>
            </Grid>
            
        </Grid>
        
    {/* <Grid container spacing={2}>
      <Grid item xs={11} md={11} height={100} >
          <Item elevation={4}>
              <h2> testing</h2>
            </Item>
      </Grid>
        <Grid item spacing={5} container >
            <Grid item xs ={12}></Grid>
            <Grid item xs ={12}></Grid>
        </Grid>
        
      
    
        <Grid item xs={7} md={4}>
            <Item>xs=7 md=4</Item>
        </Grid>
        <Grid item xs={7} md={4}>
            <Item>xs=6 md=4</Item>
        </Grid>
    
      
      <Grid item xs={6} md={8}>
        <Item>xs=6 md=8</Item>
      </Grid>
    </Grid> */}
  </Box>
   )
}

export default HomeBody;