package com.codeReview.socialmedia.service;

import com.codeReview.socialmedia.dto.PostRequest;
import com.codeReview.socialmedia.dto.PostResponse;
import com.codeReview.socialmedia.model.Post;
import com.codeReview.socialmedia.repository.PostRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.List;


@Service
@RequiredArgsConstructor
@Slf4j
public class PostService {


    private final PostRepository postRepo;


    public void createPost(PostRequest postRequest) {
        Post post = Post.builder()
                .pID(postRequest.getPostID())
                .uID(postRequest.getPostID())
                .content(postRequest.getContent())
                .build();


        postRepo.save(post);
    }


    public List<PostResponse> getAllPosts() {
        return postRepo.findAll().stream().map(post -> PostResponse.builder()
                .pID(post.getPID())
                .uID(post.getUID())
                .content(post.getContent())
                .build()).toList();
    }


}
