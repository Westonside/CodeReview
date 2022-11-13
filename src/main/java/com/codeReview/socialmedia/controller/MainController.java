package com.codeReview.socialmedia.controller;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/testing")
public class MainController {

    @PostMapping
    //indicates object has been created
    @ResponseStatus(HttpStatus.CREATED)
    public void createSomething(){

    }


    @GetMapping
    @ResponseStatus(HttpStatus.ACCEPTED)
    public void advertiseRoutes(@RequestBody Object something){

    }

}
