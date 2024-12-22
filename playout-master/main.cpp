
// Updating project to replace DirectShow with FFmpeg
// Initializing FFmpeg and SDL for video playback

extern "C" {
    #include <libavformat/avformat.h>
    #include <libavcodec/avcodec.h>
    #include <libswscale/swscale.h>
    #include <libavutil/imgutils.h>
}

#include <iostream>
#include <SDL2/SDL.h>

// Function to initialize FFmpeg and SDL
bool initializeFFmpegAndSDL(SDL_Window** window, SDL_Renderer** renderer, int width, int height) {
    av_register_all();

    if (SDL_Init(SDL_INIT_VIDEO) < 0) {
        std::cerr << "Error: Could not initialize SDL." << std::endl;
        return false;
    }

    *window = SDL_CreateWindow("FFmpeg Video Playback", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, SDL_WINDOW_SHOWN);
    if (!*window) {
        std::cerr << "Error: Could not create SDL window." << std::endl;
        SDL_Quit();
        return false;
    }

    *renderer = SDL_CreateRenderer(*window, -1, SDL_RENDERER_ACCELERATED);
    if (!*renderer) {
        std::cerr << "Error: Could not create SDL renderer." << std::endl;
        SDL_DestroyWindow(*window);
        SDL_Quit();
        return false;
    }

    return true;
}

// Function to play a video file using FFmpeg
void playVideo(const char* filename) {
    AVFormatContext* formatContext = nullptr;
    AVCodecContext* codecContext = nullptr;
    AVCodec* codec = nullptr;
    AVPacket* packet = av_packet_alloc();
    AVFrame* frame = av_frame_alloc();
    AVFrame* frameYUV = av_frame_alloc();

    SwsContext* swsContext = nullptr;
    SDL_Window* window = nullptr;
    SDL_Renderer* renderer = nullptr;
    SDL_Texture* texture = nullptr;

    // Open video file
    if (avformat_open_input(&formatContext, filename, nullptr, nullptr) != 0) {
        std::cerr << "Error: Could not open video file." << std::endl;
        return;
    }

    if (avformat_find_stream_info(formatContext, nullptr) < 0) {
        std::cerr << "Error: Could not find stream info." << std::endl;
        avformat_close_input(&formatContext);
        return;
    }

    // Find the first video stream
    int videoStreamIndex = -1;
    for (unsigned int i = 0; i < formatContext->nb_streams; i++) {
        if (formatContext->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
            videoStreamIndex = i;
            break;
        }
    }

    if (videoStreamIndex == -1) {
        std::cerr << "Error: Could not find video stream." << std::endl;
        avformat_close_input(&formatContext);
        return;
    }

    AVCodecParameters* codecParameters = formatContext->streams[videoStreamIndex]->codecpar;
    codec = avcodec_find_decoder(codecParameters->codec_id);
    if (!codec) {
        std::cerr << "Error: Unsupported codec." << std::endl;
        avformat_close_input(&formatContext);
        return;
    }

    codecContext = avcodec_alloc_context3(codec);
    if (avcodec_parameters_to_context(codecContext, codecParameters) < 0) {
        std::cerr << "Error: Could not initialize codec context." << std::endl;
        avformat_close_input(&formatContext);
        avcodec_free_context(&codecContext);
        return;
    }

    if (avcodec_open2(codecContext, codec, nullptr) < 0) {
        std::cerr << "Error: Could not open codec." << std::endl;
        avformat_close_input(&formatContext);
        avcodec_free_context(&codecContext);
        return;
    }

    // Initialize SDL
    if (!initializeFFmpegAndSDL(&window, &renderer, codecContext->width, codecContext->height)) {
        avcodec_free_context(&codecContext);
        avformat_close_input(&formatContext);
        return;
    }

    texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_YV12, SDL_TEXTUREACCESS_STREAMING, codecContext->width, codecContext->height);

    swsContext = sws_getContext(codecContext->width, codecContext->height, codecContext->pix_fmt,
                                 codecContext->width, codecContext->height, AV_PIX_FMT_YUV420P,
                                 SWS_BILINEAR, nullptr, nullptr, nullptr);

    int numBytes = av_image_get_buffer_size(AV_PIX_FMT_YUV420P, codecContext->width, codecContext->height, 1);
    uint8_t* buffer = (uint8_t*)av_malloc(numBytes * sizeof(uint8_t));
    av_image_fill_arrays(frameYUV->data, frameYUV->linesize, buffer, AV_PIX_FMT_YUV420P, codecContext->width, codecContext->height, 1);

    SDL_Event event;
    bool isRunning = true;

    while (isRunning && av_read_frame(formatContext, packet) >= 0) {
        if (packet->stream_index == videoStreamIndex) {
            if (avcodec_send_packet(codecContext, packet) == 0) {
                while (avcodec_receive_frame(codecContext, frame) == 0) {
                    sws_scale(swsContext, frame->data, frame->linesize, 0, codecContext->height, frameYUV->data, frameYUV->linesize);

                    SDL_UpdateYUVTexture(texture, nullptr,
                                         frameYUV->data[0], frameYUV->linesize[0],
                                         frameYUV->data[1], frameYUV->linesize[1],
                                         frameYUV->data[2], frameYUV->linesize[2]);

                    SDL_RenderClear(renderer);
                    SDL_RenderCopy(renderer, texture, nullptr, nullptr);
                    SDL_RenderPresent(renderer);
                }
            }
        }
        av_packet_unref(packet);

        while (SDL_PollEvent(&event)) {
            if (event.type == SDL_QUIT) {
                isRunning = false;
                break;
            }
        }
    }

    av_free(buffer);
    av_frame_free(&frame);
    av_frame_free(&frameYUV);
    sws_freeContext(swsContext);

    SDL_DestroyTexture(texture);
    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);
    SDL_Quit();

    avcodec_free_context(&codecContext);
    avformat_close_input(&formatContext);
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        std::cerr << "Usage: " << argv[0] << " <video file>" << std::endl;
        return -1;
    }

    playVideo(argv[1]);
    return 0;
}
