function  generatingSounds()
numTicks = 8; % divisions in an axis
axis = 3; % number of axes.
f0 = 220; % A3
dur = .250; % s
sr = 48000; % kHz
len = dur*sr; % length of a tone

% minor scale in cents:
minor = [0, 200, 300, 500, 700, 900, 1100, 1200]; % in cents

% harmonic amplitudes
amps = 1./(1:numTicks);


% modulation indices for sand-pile
m = linspace(0,100,6);

% harmonic phases:
phases = linspace(0,2*pi,9);

% create envelope
attack = round(.25*len);
envelope = [linspace(0,1,attack)'; linspace(1,0,len-attack)'];

% this implementation can be done using matrices for a faster result
% I'm using iterations for ease of understanding
for i=1:numTicks % x-axis: F0
    for j=1:numTicks  % y-axis: Harmonics 
        for k=1:numTicks % z-axis: Phases
            f = f0*2^(minor(i)/1200)*j;
            % taking care of the game of life first
            tone = amps(j).*cos(2*pi*f.*linspace(0,dur,len)+phases(k));
            tone = envelope.*tone';
            audiowrite([num2str(i) '_' num2str(j) '_' num2str(k) '.wav'], ...
                tone,sr);
            % Now doing AM for the sand-pile
            for l = 1:6
            tone = (1+m(l).*cos(2*pi*f/l*linspace(0,dur,len))).*cos(2*pi*f.*linspace(0,dur,len)+phases(k));
            tone = envelope.*(amps(j).*tone'/max(abs(tone(:))));
            audiowrite(['pos_' num2str(i) '_' num2str(j) '_' num2str(k)  '_' num2str(l) '.wav'], ...
                tone,sr);
            end
            
        end
    end
end








end

