import com.codeReview.socialmedia.model.Post;
import com.codeReview.socialmedia.model.User;
import com.codeReview.socialmedia.repository.PostRepository;
import com.codeReview.socialmedia.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;

@Service
public class PostService {

    @Autowired
    PostRepository postRepository;

    @Autowired
    //UserRepository userRepository;


    public Post savePost(String content){
        Post post = new Post();
        User user = userRepository.findUser(user.getUsername());
        post.setUser(user);
        post.setPost(content);
        return postRepository.save(post);
    }

}