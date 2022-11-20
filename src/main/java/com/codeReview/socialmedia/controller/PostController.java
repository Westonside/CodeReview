package com.codeReview.socialmedia.controller;

import com.codeReview.socialmedia.dto.PostRequest;
import com.codeReview.socialmedia.dto.PostResponse;
import com.codeReview.socialmedia.service.PostService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/posts")
@RequiredArgsConstructor
public class PostController {
    private final PostService postService;

    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public void createPost(@RequestBody PostRequest postrequest) {
        postService.createPost(postrequest);
    }

    @GetMapping
    @ResponseStatus(HttpStatus.OK)
    public List<PostResponse> getAllPost() {
        return postService.getAllPosts();
    }
}
